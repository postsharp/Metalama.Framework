// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Executes aspects.
    /// </summary>
    internal class AspectDriver : IAspectDriver
    {
        private readonly ReflectionMapper _reflectionMapper;
        private readonly IAspectClassImpl _aspectClass;

        public IEligibilityRule<IDeclaration>? EligibilityRule { get; }

        public AspectDriver( IServiceProvider serviceProvider, IAspectClassImpl aspectClass, Compilation compilation )
        {
            this._reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( compilation );
            this._aspectClass = aspectClass;

            // We don't store the IServiceProvider because the AspectDriver is created during the pipeline initialization but used
            // during pipeline execution, and execution has a different service provider.

            // Introductions must have a deterministic order because of testing.
            var declarativeAdviceAttributes = aspectClass
                .TemplateClasses.SelectMany( c => c.GetDeclarativeAdvices( serviceProvider, compilation ) )
                .ToImmutableArray();

            // If we have any declarative introduction, the aspect cannot be added to an interface.
            foreach ( var declarativeAdvice in declarativeAdviceAttributes )
            {
                var eligibilityBuilder = new EligibilityBuilder<IDeclaration>();
                declarativeAdvice.Attribute.BuildAspectEligibility( eligibilityBuilder );
                this.EligibilityRule = eligibilityBuilder.Build();
            }
        }

        public AspectInstanceResult ExecuteAspect(
            IAspectInstanceInternal aspectInstance,
            CompilationModel initialCompilationRevision,
            CompilationModel currentCompilationRevision,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
        {
            var target = aspectInstance.TargetDeclaration.GetTarget( initialCompilationRevision );

            return target switch
            {
                ICompilation compilation => this.EvaluateAspect(
                    compilation,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                INamedType type => this.EvaluateAspect(
                    type,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IMethod method => this.EvaluateAspect(
                    method,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IField field => this.EvaluateAspect(
                    field,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IProperty property => this.EvaluateAspect(
                    property,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IConstructor constructor => this.EvaluateAspect(
                    constructor,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IParameter parameter => this.EvaluateAspect(
                    parameter,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                ITypeParameter genericParameter => this.EvaluateAspect(
                    genericParameter,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IEvent @event => this.EvaluateAspect(
                    @event,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                INamespace ns => this.EvaluateAspect(
                    ns,
                    aspectInstance,
                    initialCompilationRevision,
                    currentCompilationRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                _ => throw new NotSupportedException( $"Cannot add an aspect to a declaration of type {target.DeclarationKind}." )
            };
        }

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            IAspectInstanceInternal aspectInstance,
            CompilationModel initialCompilationRevision,
            CompilationModel currentCompilationRevision,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
            where T : class, IDeclaration
        {
            AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
            {
                return new AspectInstanceResult(
                    aspectInstance,
                    AdviceOutcome.Error,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<ITransformation>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serviceProvider = pipelineConfiguration.ServiceProvider;

            // Map the target declaration to the correct revision of the compilation model.
            targetDeclaration = initialCompilationRevision.Factory.GetDeclaration( targetDeclaration );

            if ( aspectInstance.Aspect is not IAspect<T> aspectOfT )
            {
                // TODO: should the diagnostic be applied to the attribute, if one exists?

                // Get the code model type for the reflection type so we have better formatting of the diagnostic.
                var interfaceType = this._reflectionMapper.GetTypeSymbol( typeof(IAspect<T>) ).AssertNotNull();

                var diagnostic =
                    GeneralDiagnosticDescriptors.AspectAppliedToIncorrectDeclaration.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (AspectType: this._aspectClass.ShortName, targetDeclaration.DeclarationKind, targetDeclaration, interfaceType) );

                return CreateResultForError( diagnostic );
            }

            var diagnosticSink = new UserDiagnosticSink( this._aspectClass.Project, pipelineConfiguration.CodeFixFilter );

            // Create the AdviceFactory.
            var adviceFactoryState = new AdviceFactoryState(
                serviceProvider,
                initialCompilationRevision,
                currentCompilationRevision,
                aspectInstance,
                diagnosticSink,
                pipelineConfiguration );

            var adviceFactory = new AdviceFactory(
                adviceFactoryState,
                aspectInstance.TemplateInstances.Count == 1 ? aspectInstance.TemplateInstances.Values.Single() : null,
                null );

            // Prepare declarative advice.
            var declarativeAdvice = this._aspectClass.TemplateClasses
                .SelectMany( c => c.GetDeclarativeAdvices( serviceProvider, initialCompilationRevision ) )
                .ToList();

            // Create the AspectBuilder.
            var aspectBuilderState = new AspectBuilderState(
                serviceProvider,
                diagnosticSink,
                pipelineConfiguration,
                aspectInstance,
                adviceFactoryState,
                cancellationToken );

            var aspectBuilder = new AspectBuilder<T>( targetDeclaration, aspectBuilderState, adviceFactory );

            adviceFactoryState.AspectBuilder = aspectBuilder;

            using ( SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( initialCompilationRevision, serviceProvider ) ) )
            {
                var executionContext = new UserCodeExecutionContext(
                    serviceProvider,
                    diagnosticSink,
                    UserCodeMemberInfo.FromDelegate( new Action<IAspectBuilder<T>>( aspectOfT.BuildAspect ) ),
                    new AspectLayerId( this._aspectClass ),
                    initialCompilationRevision,
                    targetDeclaration );

                if ( !serviceProvider.GetRequiredService<UserCodeInvoker>()
                        .TryInvoke(
                            () =>
                            {
                                // Execute declarative advice.
                                foreach ( var advice in declarativeAdvice )
                                {
                                    ((DeclarativeAdviceAttribute) advice.AdviceAttribute.AssertNotNull()).BuildAspect(
                                        advice.Declaration!,
                                        advice.TemplateClassMember.Key,
                                        aspectBuilder );
                                }

                                if ( !aspectBuilder.IsAspectSkipped )
                                {
                                    aspectOfT.BuildAspect( aspectBuilder );
                                }
                            },
                            executionContext ) )
                {
                    aspectInstance.Skip();

                    return
                        new AspectInstanceResult(
                            aspectInstance,
                            AdviceOutcome.Error,
                            diagnosticSink.ToImmutable(),
                            ImmutableArray<ITransformation>.Empty,
                            ImmutableArray<IAspectSource>.Empty,
                            ImmutableArray<IValidatorSource>.Empty );
                }

                var aspectResult = aspectBuilderState.ToResult();

                if ( aspectResult.Outcome == AdviceOutcome.Error )
                {
                    aspectInstance.Skip();
                }
                else
                {
                    // Validators on the current version of the compilation must be executed now.

                    if ( !aspectResult.ValidatorSources.IsDefaultOrEmpty )
                    {
                        diagnosticSink.Reset();

                        var validationRunner = new ValidationRunner( pipelineConfiguration, aspectResult.ValidatorSources, CancellationToken.None );
                        validationRunner.RunDeclarationValidators( initialCompilationRevision, CompilationModelVersion.Current, diagnosticSink );

                        if ( !diagnosticSink.IsEmpty )
                        {
                            aspectResult = aspectResult.WithAdditionalDiagnostics( diagnosticSink.ToImmutable() );
                        }
                    }
                }

                return aspectResult;
            }
        }
    }
}