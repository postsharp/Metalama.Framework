// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.DependencyInjection;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.Advising;

/*
internal abstract class IntroduceFieldOrPropertyAdvice<TMember, TBuilder> : IntroduceMemberAdvice<TMember, TBuilder>
    where TMember : class, IFieldOrProperty
    where TBuilder : MemberBuilder, IFieldOrPropertyBuilder
{
    protected IntroduceFieldOrPropertyAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        INamedType targetDeclaration,
        string? explicitName,
        TemplateMember<TMember> template,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        ICompilation sourceCompilation,
        Action<TBuilder>? buildAction,
        string? layerName,
        IObjectReader tags )
        : base( aspect, templateInstance, targetDeclaration, sourceCompilation, explicitName, template, scope, overrideStrategy, buildAction, layerName, tags )
    {
    }

    /// <summary>
    /// Creates an <see cref="AdviceImplementationResult"/> that introduces the <see cref="MemberBuilder"/> and appends any transformation necessary to
    /// pull the new member from the constructor.
    /// </summary>
    /// <returns></returns>
    protected AdviceImplementationResult IntroduceMemberAndPull(
        IServiceProvider serviceProvider,
        INamedType targetType,
        Action<ITransformation> addTransformation,
        AdviceOutcome outcome )
    {
        addTransformation( this.Builder );

        if ( this.PullStrategy == null )
        {
            // The field or property should not be pulled or initialized.
            return AdviceImplementationResult.Success( outcome, this.Builder );
        }
        else
        {
            var userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            var syntaxGenerationContextFactory = serviceProvider.GetRequiredService<SyntaxGenerationContextFactory>();
            var userCodeInvocationContext = new UserCodeExecutionContext( serviceProvider, this.AspectLayerId, targetType.GetCompilationModel(), targetType );

            UserDiagnosticSink diagnosticSink = new();
            var scopedDiagnosticSink = new ScopedDiagnosticSink( diagnosticSink, targetType, targetType );

            var compilation = targetType.GetCompilationModel();

            // Find all constructors except those who call `: this(...)`.
            foreach ( var constructor in targetType.Constructors )
            {
                if ( constructor.InitializerKind != ConstructorInitializerKind.This )
                {
                    // Invoke the IPullStrategy.
                    PullAction pullFieldOrPropertyAction;

                    var syntaxGenerationContext = syntaxGenerationContextFactory.GetSyntaxGenerationContext(
                        constructor.GetPrimaryDeclarationSyntax() ?? constructor.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

                    using ( SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( compilation, syntaxGenerationContext ) ) )
                    {
                        pullFieldOrPropertyAction = userCodeInvoker.Invoke(
                            () => this.PullStrategy.PullFieldOrProperty(
                                this.Builder,
                                constructor,
                                scopedDiagnosticSink ),
                            userCodeInvocationContext );
                    }

                    if ( diagnosticSink.ErrorCount > 0 )
                    {
                        return AdviceImplementationResult.Failed( diagnosticSink );
                    }

                    switch ( pullFieldOrPropertyAction.Kind )
                    {
                        case DependencyPullStrategyKind.DoNotPull:
                            // Add the member but does not pull.
                            break;

                        case DependencyPullStrategyKind.UseExistingParameter:
                        case DependencyPullStrategyKind.AppendParameterAndPull:
                            var initializedConstructor = constructor;

                            // If we have an implicit constructor, make it explicit.
                            if ( constructor.IsImplicitInstanceConstructor() )
                            {
                                var constructorBuilder = new ConstructorBuilder( this, constructor.DeclaringType, this.Tags );
                                initializedConstructor = constructorBuilder;
                                addTransformation( constructorBuilder );
                            }

                            // Add an initializer for the field or property into the constructor.
                            var initialization = new SyntaxBasedInitializationTransformation(
                                this,
                                initializedConstructor,
                                initializedConstructor,
                                context => SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName( this.Builder.Name ) ),
                                            ((IUserExpression?) pullFieldOrPropertyAction.AssignmentExpression)?.ToSyntax( context )
                                            ?? SyntaxFactory.IdentifierName( pullFieldOrPropertyAction.ParameterName.AssertNotNull() ) ) )
                                    .NormalizeWhitespace()
                                    .WithAdditionalAnnotations( this.Aspect.AspectClass.GeneratedCodeAnnotation ),
                                this.Tags );

                            addTransformation( initialization );

                            // Pull the parameter from the constructor.
                            if ( pullFieldOrPropertyAction.Kind == DependencyPullStrategyKind.AppendParameterAndPull )
                            {
                                // Create the parameter.
                                var newParameter = new ParameterBuilder(
                                    this,
                                    initializedConstructor,
                                    initializedConstructor.Parameters.Count,
                                    pullFieldOrPropertyAction.ParameterName.AssertNotNull(),
                                    pullFieldOrPropertyAction.ParameterType.AssertNotNull(),
                                    RefKind.None );

                                newParameter.AddAttributes( pullFieldOrPropertyAction.ParameterAttributes );

                                addTransformation( new AppendParameterTransformation( this, newParameter ) );

                                // Pull from constructors that call the current constructor, and recursively.
                                var pullResult = PullConstructorParameterRecursive( constructor, newParameter );

                                if ( pullResult != null )
                                {
                                    return pullResult;
                                }

                                AdviceImplementationResult? PullConstructorParameterRecursive( IConstructor baseConstructor, IParameter baseParameter )
                                {
                                    // Identity constructors that call the current constructor.
                                    var sameTypeConstructors =
                                        baseConstructor.DeclaringType.Constructors.Where( c => c.InitializerKind == ConstructorInitializerKind.This );

                                    var derivedConstructors = compilation
                                        .GetDerivedTypes( baseConstructor.DeclaringType, false )
                                        .SelectMany( t => t.Constructors )
                                        .Where( c => c.InitializerKind != ConstructorInitializerKind.This );

                                    var chainedConstructors =
                                        sameTypeConstructors.Concat( derivedConstructors )
                                            .Where( c => ((IConstructorImpl) c).GetBaseConstructor() == baseConstructor );

                                    // Process all of these constructors.
                                    foreach ( var chainedConstructor in chainedConstructors )
                                    {
                                        var chainedSyntaxGenerationContext = syntaxGenerationContextFactory.GetSyntaxGenerationContext(
                                            constructor.GetPrimaryDeclarationSyntax()
                                            ?? constructor.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

                                        PullAction pullParameterAction;

                                        using ( SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( compilation, chainedSyntaxGenerationContext ) ) )
                                        {
                                            // Ask the IPullStrategy what to do.
                                            pullParameterAction = this.PullStrategy.PullParameter(
                                                baseParameter,
                                                chainedConstructor,
                                                scopedDiagnosticSink );
                                        }

                                        if ( diagnosticSink.ErrorCount > 0 )
                                        {
                                            return AdviceImplementationResult.Failed( diagnosticSink );
                                        }

                                        // If we have an implicit constructor, make it explicit.
                                        var initializedChainedConstructor = chainedConstructor;

                                        if ( chainedConstructor.IsImplicitInstanceConstructor() )
                                        {
                                            var derivedConstructorBuilder = new ConstructorBuilder( this, chainedConstructor.DeclaringType, this.Tags );
                                            addTransformation( derivedConstructorBuilder );
                                            initializedChainedConstructor = derivedConstructorBuilder;
                                        }

                                        // Execute the strategy.
                                        ExpressionSyntax parameterValue;

                                        switch ( pullParameterAction.Kind )
                                        {
                                            case DependencyPullStrategyKind.DoNotPull:
                                                parameterValue = SyntaxFactoryEx.Default;

                                                break;

                                            case DependencyPullStrategyKind.UseExistingParameter:
                                                parameterValue = SyntaxFactory.IdentifierName( pullParameterAction.ParameterName.AssertNotNull() );

                                                break;

                                            case DependencyPullStrategyKind.AppendParameterAndPull:
                                                // Create a new parameter.
                                                parameterValue = SyntaxFactory.IdentifierName( pullParameterAction.ParameterName.AssertNotNull() );

                                                var recursiveNewParameter = new ParameterBuilder(
                                                    this,
                                                    initializedChainedConstructor,
                                                    initializedChainedConstructor.Parameters.Count,
                                                    pullParameterAction.ParameterName.AssertNotNull(),
                                                    pullParameterAction.ParameterType.AssertNotNull(),
                                                    RefKind.None );

                                                recursiveNewParameter.AddAttributes( pullParameterAction.ParameterAttributes );

                                                addTransformation( new AppendParameterTransformation( this, recursiveNewParameter ) );

                                                // Process all constructors calling this constructor.
                                                var recursiveResult = PullConstructorParameterRecursive( chainedConstructor, recursiveNewParameter );

                                                if ( recursiveResult != null )
                                                {
                                                    return recursiveResult;
                                                }

                                                break;

                                            default:
                                                throw new AssertionFailedException();
                                        }

                                        // Append an argument to the call to the current constructor. 
                                        addTransformation(
                                            new AppendConstructorInitializerArgumentTransformation(
                                                this,
                                                initializedChainedConstructor,
                                                baseParameter.Index,
                                                parameterValue ) );
                                    }

                                    return null;
                                }
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }
            }

            return AdviceImplementationResult.Success( this.Builder );
        }
    }
}
*/