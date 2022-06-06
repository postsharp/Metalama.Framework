// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.Advices;

internal abstract class IntroduceFieldOrPropertyAdvice<TMember, TBuilder> : IntroduceMemberAdvice<TMember, TBuilder>
    where TMember : class, IFieldOrProperty
    where TBuilder : MemberBuilder, IFieldOrPropertyBuilder
{
    public IPullStrategy? PullStrategy { get; }

    protected IntroduceFieldOrPropertyAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        INamedType targetDeclaration,
        string? explicitName,
        TemplateMember<TMember> template,
        IntroductionScope scope,
        OverrideStrategy overrideStrategy,
        string? layerName,
        IObjectReader tags,
        IPullStrategy? pullStrategy )
        : base( aspect, templateInstance, targetDeclaration, explicitName, template, scope, overrideStrategy, layerName, tags )
    {
        this.PullStrategy = pullStrategy;
    }

    /// <summary>
    /// Creates an <see cref="AdviceResult"/> that introduces the <see cref="MemberBuilder"/> and appends any transformation necessary to
    /// pull the new member from the constructor.
    /// </summary>
    /// <returns></returns>
    protected AdviceResult IntroduceMemberAndPull( INamedType targetType )
    {
        if ( this.PullStrategy == null )
        {
            return AdviceResult.Create( this.MemberBuilder );
        }
        else
        {
            UserDiagnosticSink diagnosticSink = new();
            var scopedDiagnosticSink = new ScopedDiagnosticSink( diagnosticSink, targetType, targetType );

            var transformations = new List<ITransformation>();
            transformations.Add( this.MemberBuilder );

            foreach ( var constructor in targetType.Constructors )
            {
                if ( constructor.InitializerKind == ConstructorInitializerKind.Base )
                {
                    // TODO: use UserCodeInvoker.

                    var pullFieldOrPropertyAction = this.PullStrategy.PullFieldOrProperty(
                        this.MemberBuilder,
                        constructor,
                        scopedDiagnosticSink );

                    switch ( pullFieldOrPropertyAction.Kind )
                    {
                        case DependencyPullStrategyKind.DoNotPull:
                            // Add the member but does not pull.
                            break;

                        case DependencyPullStrategyKind.UseExistingParameter:
                        case DependencyPullStrategyKind.AppendParameterAndPull:
                            var initializedConstructor = constructor;

                            if ( constructor.IsImplicitInstanceConstructor() )
                            {
                                var constructorBuilder = new ConstructorBuilder( this, constructor.DeclaringType, this.Tags );
                                initializedConstructor = constructorBuilder;
                            }

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
                                                SyntaxFactory.IdentifierName( this.MemberBuilder.Name ) ),
                                            ((IUserExpression?) pullFieldOrPropertyAction.AssignmentExpression)?.ToSyntax( context )
                                            ?? SyntaxFactory.IdentifierName( pullFieldOrPropertyAction.ParameterName.AssertNotNull() ) ) )
                                    .NormalizeWhitespace(),
                                this.Tags );

                            transformations.Add( initialization );

                            if ( pullFieldOrPropertyAction.Kind == DependencyPullStrategyKind.AppendParameterAndPull )
                            {
                                var newParameter = new ParameterBuilder(
                                    this,
                                    initializedConstructor,
                                    initializedConstructor.Parameters.Count,
                                    pullFieldOrPropertyAction.ParameterName.AssertNotNull(),
                                    pullFieldOrPropertyAction.ParameterType.AssertNotNull(),
                                    RefKind.None );

                                // TODO: UserCodeInvoker
                                pullFieldOrPropertyAction.BuildParameterAction?.Invoke( newParameter );

                                transformations.Add( newParameter );

                                var compilation = targetType.GetCompilationModel();
                                PullConstructorParameterRecursive( constructor, newParameter );

                                void PullConstructorParameterRecursive( IConstructor baseConstructor, IParameter baseParameter )
                                {
                                    var sameTypeConstructors =
                                        baseConstructor.DeclaringType.Constructors.Where( c => c.InitializerKind == ConstructorInitializerKind.This );
                                    
                                    var derivedConstructors = compilation
                                        .GetDerivedTypes( baseConstructor.DeclaringType, false )
                                        .SelectMany( t => t.Constructors )
                                        .Where( c => c.InitializerKind != ConstructorInitializerKind.This );

                                    var chainedConstructors =
                                        sameTypeConstructors.Concat( derivedConstructors )
                                            .Where( c => ((IConstructorImpl) c).GetBaseConstructor() == baseConstructor );
                                        
                                    foreach ( var chainedConstructor in chainedConstructors )
                                    {
                                        if ( chainedConstructor.InitializerKind == ConstructorInitializerKind.Base
                                             && ((IConstructorImpl) chainedConstructor).GetBaseConstructor() == baseConstructor )
                                        {
                                            var pullParameterAction = this.PullStrategy.PullParameter(
                                                baseParameter,
                                                chainedConstructor,
                                                scopedDiagnosticSink );

                                            var initializedChainedConstructor = chainedConstructor;

                                            if ( chainedConstructor.IsImplicitInstanceConstructor() )
                                            {
                                                var derivedConstructorBuilder = new ConstructorBuilder( this, constructor.DeclaringType, this.Tags );
                                                initializedChainedConstructor = derivedConstructorBuilder;
                                            }

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
                                                    parameterValue = SyntaxFactory.IdentifierName( pullParameterAction.ParameterName.AssertNotNull() );

                                                    var recursiveNewParameter = new ParameterBuilder(
                                                        this,
                                                        initializedChainedConstructor,
                                                        initializedChainedConstructor.Parameters.Count,
                                                        pullParameterAction.ParameterName.AssertNotNull(),
                                                        pullParameterAction.ParameterType.AssertNotNull(),
                                                        RefKind.None );
                                                    
                                                    pullParameterAction.BuildParameterAction?.Invoke( recursiveNewParameter );

                                                    transformations.Add( recursiveNewParameter );

                                                    PullConstructorParameterRecursive( chainedConstructor, recursiveNewParameter );

                                                    break;

                                                default:
                                                    throw new AssertionFailedException();
                                            }

                                            transformations.Add(
                                                new AppendConstructorInitializerArgumentTransformation(
                                                    this,
                                                    initializedChainedConstructor,
                                                    baseParameter.Index,
                                                    parameterValue ) );
                                        }
                                    }
                                }
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }
            }

            return AdviceResult.Create( transformations, diagnosticSink.ToImmutable().ReportedDiagnostics );
        }
    }
}