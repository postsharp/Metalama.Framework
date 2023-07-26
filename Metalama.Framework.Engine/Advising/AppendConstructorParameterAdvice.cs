// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal sealed class AppendConstructorParameterAdvice : Advice
{
    private readonly string _parameterName;
    private readonly IType _parameterType;
    private readonly Action<ParameterBuilder>? _buildAction;
    private readonly Func<IParameter, IConstructor, PullAction>? _pullActionFunc;
    private readonly TypedConstant _defaultValue;

    public AppendConstructorParameterAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        string? layerName,
        string parameterName,
        IType parameterType,
        Action<ParameterBuilder>? buildAction,
        Func<IParameter, IConstructor, PullAction>? pullActionFunc,
        TypedConstant defaultValue ) : base( aspect, template, targetDeclaration, sourceCompilation, layerName )
    {
        this._parameterName = parameterName;
        this._parameterType = parameterType;
        this._buildAction = buildAction;
        this._pullActionFunc = pullActionFunc;
        this._defaultValue = defaultValue;
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceParameter;

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var syntaxGenerationContextFactory = compilation.CompilationContext.SyntaxGenerationContextFactory;

        var constructor = (IConstructor) this.TargetDeclaration.GetTarget( compilation );
        var initializedConstructor = constructor;

        // Introducing parameters into static constructors is not allowed.
        if ( constructor.IsStatic )
        {
            return AdviceImplementationResult.Failed(
                AdviceDiagnosticDescriptors.CannotIntroduceParameterIntoStaticConstructor.CreateRoslynDiagnostic(
                    constructor.GetDiagnosticLocation(),
                    (this.Aspect.AspectClass.ShortName, constructor),
                    this ) );
        }

        // Introducing parameters into anything else than a class is not allowed.
        if ( constructor.DeclaringType.TypeKind != TypeKind.Class )
        {
            return AdviceImplementationResult.Failed(
                AdviceDiagnosticDescriptors.CannotIntroduceParameterIntoNonClassConstructor.CreateRoslynDiagnostic(
                    constructor.GetDiagnosticLocation(),
                    (this.Aspect.AspectClass.ShortName, constructor),
                    this ) );
        }

        // If we have an implicit constructor, make it explicit.
        if ( constructor.IsImplicitInstanceConstructor() )
        {
            var constructorBuilder = new ConstructorBuilder( constructor.DeclaringType, this );
            initializedConstructor = constructorBuilder;
            addTransformation( constructorBuilder.ToTransformation() );
        }

        // Create the parameter.
        var parameterBuilder = new ParameterBuilder(
            initializedConstructor,
            initializedConstructor.Parameters.Count,
            this._parameterName,
            this._parameterType,
            RefKind.None,
            this ) { DefaultValue = this._defaultValue };

        var parameter = parameterBuilder.ForCompilation( compilation, ReferenceResolutionOptions.CanBeMissing );

        this._buildAction?.Invoke( parameterBuilder );

        addTransformation( new IntroduceParameterTransformation( this, parameterBuilder ) );

        // Pull from constructors that call the current constructor, and recursively.
        PullConstructorParameterRecursive( constructor, parameter );

        return AdviceImplementationResult.Success( parameterBuilder );

        void PullConstructorParameterRecursive( IConstructor baseConstructor, IParameter baseParameter )
        {
            // Identity constructors that call the current constructor.
            var sameTypeConstructors =
                baseConstructor.DeclaringType.Constructors.Where( c => c.InitializerKind == ConstructorInitializerKind.This );

            var derivedConstructors = compilation
                .GetDerivedTypes( baseConstructor.DeclaringType, DerivedTypesOptions.DirectOnly )
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

                if ( this._pullActionFunc != null )
                {
                    using ( UserCodeExecutionContext.WithContext( serviceProvider, compilation ) )
                    {
                        // Ask the IPullStrategy what to do.
                        pullParameterAction = this._pullActionFunc( parameterBuilder, chainedConstructor );
                    }
                }
                else
                {
                    pullParameterAction = PullAction.None;
                }

                // If we have an implicit constructor, make it explicit.
                var initializedChainedConstructor = chainedConstructor;

                if ( chainedConstructor.IsImplicitInstanceConstructor() )
                {
                    var derivedConstructorBuilder = new ConstructorBuilder( chainedConstructor.DeclaringType, this );
                    addTransformation( derivedConstructorBuilder.ToTransformation() );
                    initializedChainedConstructor = derivedConstructorBuilder;
                }

                // Execute the strategy.
                ExpressionSyntax parameterValue;

                switch ( pullParameterAction.Kind )
                {
                    case PullActionKind.DoNotPull:
                        // We do not add a new argument and reply on the optional value.
                        continue;

                    case PullActionKind.UseExpression:
                        parameterValue =
                            pullParameterAction.Expression.AssertNotNull().ToExpressionSyntax( chainedSyntaxGenerationContext );

                        break;

                    case PullActionKind.AppendParameterAndPull:
                        // Create a new parameter.
                        parameterValue = SyntaxFactory.IdentifierName( pullParameterAction.ParameterName.AssertNotNull() );

                        var recursiveParameterBuilder = new ParameterBuilder(
                            initializedChainedConstructor,
                            initializedChainedConstructor.Parameters.Count,
                            pullParameterAction.ParameterName.AssertNotNull(),
                            pullParameterAction.ParameterType.AssertNotNull(),
                            RefKind.None,
                            this ) { DefaultValue = pullParameterAction.ParameterDefaultValue };

                        recursiveParameterBuilder.AddAttributes( pullParameterAction.ParameterAttributes );

                        addTransformation( new IntroduceParameterTransformation( this, recursiveParameterBuilder ) );

                        var recursiveParameter = recursiveParameterBuilder.ForCompilation( compilation, ReferenceResolutionOptions.CanBeMissing );

                        // Process all constructors calling this constructor.
                        PullConstructorParameterRecursive( chainedConstructor, recursiveParameter );

                        break;

                    default:
                        throw new AssertionFailedException( $"Invalid value for PullActionKind: {pullParameterAction.Kind}." );
                }

                // Append an argument to the call to the current constructor. 
                addTransformation(
                    new IntroduceConstructorInitializerArgumentTransformation(
                        this,
                        initializedChainedConstructor,
                        baseParameter.Index,
                        parameterValue ) );
            }
        }
    }
}