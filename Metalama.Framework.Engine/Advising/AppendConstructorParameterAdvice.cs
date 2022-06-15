﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.DependencyInjection;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal class AppendConstructorParameterAdvice : Advice
{
    private readonly string _parameterName;
    private readonly IType _parameterType;
    private readonly Action<ParameterBuilder>? _buildAction;
    private readonly Func<IConstructor, PullAction> _pullActionFunc;

    public AppendConstructorParameterAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        string? layerName,
        string parameterName,
        IType parameterType,
        Action<ParameterBuilder>? buildAction,
        Func<IConstructor, PullAction> pullActionFunc ) : base( aspect, template, targetDeclaration, sourceCompilation, layerName )
    {
        this._parameterName = parameterName;
        this._parameterType = parameterType;
        this._buildAction = buildAction;
        this._pullActionFunc = pullActionFunc;
    }

    public override AdviceImplementationResult Implement(
        IServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var syntaxGenerationContextFactory = serviceProvider.GetRequiredService<SyntaxGenerationContextFactory>();

        var constructor = (IConstructor) this.TargetDeclaration.GetTarget( compilation );
        var initializedConstructor = constructor;

        // If we have an implicit constructor, make it explicit.
        if ( constructor.IsImplicitInstanceConstructor() )
        {
            var constructorBuilder = new ConstructorBuilder( this, constructor.DeclaringType, ObjectReader.Empty );
            initializedConstructor = constructorBuilder;
            addTransformation( constructorBuilder );
        }

        // Create the parameter.
        var newParameter = new ParameterBuilder(
            this,
            initializedConstructor,
            initializedConstructor.Parameters.Count,
            this._parameterName,
            this._parameterType,
            RefKind.None );

        this._buildAction?.Invoke( newParameter );

        addTransformation( new AppendParameterTransformation( this, newParameter ) );

        // Pull from constructors that call the current constructor, and recursively.
        PullConstructorParameterRecursive( constructor, newParameter );

        return AdviceImplementationResult.Success( initializedConstructor );

        void PullConstructorParameterRecursive( IConstructor baseConstructor, IParameter baseParameter )
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
                    pullParameterAction = this._pullActionFunc( chainedConstructor );
                }

                // If we have an implicit constructor, make it explicit.
                var initializedChainedConstructor = chainedConstructor;

                if ( chainedConstructor.IsImplicitInstanceConstructor() )
                {
                    var derivedConstructorBuilder = new ConstructorBuilder( this, chainedConstructor.DeclaringType, ObjectReader.Empty );
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
                        PullConstructorParameterRecursive( chainedConstructor, recursiveNewParameter );

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
        }
    }
}