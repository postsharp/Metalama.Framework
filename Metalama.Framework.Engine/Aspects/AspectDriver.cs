// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Eligibility.Implementation;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Templating.MetaModel;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ImmutableArray<TemplateClassMember> _declarativeAdviceAttributes;
        private readonly ReflectionMapper _reflectionMapper;
        private readonly IAspectClassImpl _aspectClass;

        public IEligibilityRule<IDeclaration>? EligibilityRule { get; }

        public AspectDriver( IServiceProvider serviceProvider, IAspectClassImpl aspectClass, Compilation compilation )
        {
            this._serviceProvider = serviceProvider;
            this._reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( compilation );
            this._aspectClass = aspectClass;

            // Introductions must have a deterministic order because of testing.
            this._declarativeAdviceAttributes = aspectClass
                .TemplateClasses.SelectMany( c => c.GetDeclarativeAdvices() )
                .ToImmutableArray();

            // If we have any declarative introduction, the aspect cannot be added to an interface.
            foreach ( var declarativeAdvice in this._declarativeAdviceAttributes )
            {
                var eligibilityBuilder = new EligibilityBuilder<IDeclaration>();
                ((DeclarativeAdviceAttribute) declarativeAdvice.TemplateInfo.Attribute).BuildEligibility( eligibilityBuilder );
                this.EligibilityRule = eligibilityBuilder.Build();
            }
        }

        public AspectInstanceResult ExecuteAspect(
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
        {
            var target = aspectInstance.TargetDeclaration.GetTarget( compilationModelRevision );

            return target switch
            {
                ICompilation compilation => this.EvaluateAspect(
                    compilation,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                INamedType type => this.EvaluateAspect( type, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IMethod method => this.EvaluateAspect( method, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IField field => this.EvaluateAspect( field, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IProperty property => this.EvaluateAspect( property, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                IConstructor constructor => this.EvaluateAspect(
                    constructor,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IParameter parameter => this.EvaluateAspect( parameter, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                ITypeParameter genericParameter => this.EvaluateAspect(
                    genericParameter,
                    aspectInstance,
                    compilationModelRevision,
                    pipelineConfiguration,
                    cancellationToken ),
                IEvent @event => this.EvaluateAspect( @event, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                INamespace ns => this.EvaluateAspect( ns, aspectInstance, compilationModelRevision, pipelineConfiguration, cancellationToken ),
                _ => throw new NotSupportedException( $"Cannot add an aspect to a declaration of type {target.DeclarationKind}." )
            };
        }

        private AspectInstanceResult EvaluateAspect<T>(
            T targetDeclaration,
            IAspectInstanceInternal aspectInstance,
            CompilationModel compilationModelRevision,
            AspectPipelineConfiguration pipelineConfiguration,
            CancellationToken cancellationToken )
            where T : class, IDeclaration
        {
            AspectInstanceResult CreateResultForError( Diagnostic diagnostic )
            {
                return new AspectInstanceResult(
                    aspectInstance,
                    false,
                    new ImmutableUserDiagnosticList( ImmutableArray.Create( diagnostic ), ImmutableArray<ScopedSuppression>.Empty ),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Map the target declaration to the correct revision of the compilation model.
            targetDeclaration = compilationModelRevision.Factory.GetDeclaration( targetDeclaration );

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
            var adviceFactory = new AdviceFactory(
                compilationModelRevision,
                diagnosticSink,
                aspectInstance,
                aspectInstance.TemplateInstances.Count == 1 ? aspectInstance.TemplateInstances.Values.Single() : null,
                this._serviceProvider );

            // Prepare declarative advice.
            var declarativeAdvice = this._aspectClass.TemplateClasses.SelectMany( c => c.GetDeclarativeAdvices() )
                .Select(
                    a =>
                        (TemplateDeclaration: (IMemberOrNamedType) compilationModelRevision.Factory.GetDeclaration( a.Symbol ),
                         TemplateId: a.SymbolDocumentationId,
                         Attribute: (DeclarativeAdviceAttribute) a.TemplateInfo.Attribute) )
                .ToList();

            // Create the AspectBuilder.
            var aspectBuilder = new AspectBuilder<T>(
                targetDeclaration,
                diagnosticSink,
                adviceFactory,
                pipelineConfiguration,
                aspectInstance,
                cancellationToken );

            using ( SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( compilationModelRevision, OurSyntaxGenerator.Default ) ) )
            {
                var executionContext = new UserCodeExecutionContext(
                    this._serviceProvider,
                    diagnosticSink,
                    UserCodeMemberInfo.FromDelegate( new Action<IAspectBuilder<T>>( aspectOfT.BuildAspect ) ),
                    new AspectLayerId( this._aspectClass ),
                    compilationModelRevision,
                    targetDeclaration );

                var success = true;

                if ( !this._serviceProvider.GetRequiredService<UserCodeInvoker>()
                        .TryInvoke(
                            () =>
                            {
                                // Execute declarative advice.
                                foreach ( var advice in declarativeAdvice )
                                {
                                    success |= advice.Attribute.TryBuildAspect( advice.TemplateDeclaration, advice.TemplateId, aspectBuilder );
                                }

                                if ( success )
                                {
                                    aspectOfT.BuildAspect( aspectBuilder );
                                }
                            },
                            executionContext ) || !success )
                {
                    aspectInstance.Skip();

                    return
                        new AspectInstanceResult(
                            aspectInstance,
                            false,
                            diagnosticSink.ToImmutable(),
                            ImmutableArray<Advice>.Empty,
                            ImmutableArray<IAspectSource>.Empty,
                            ImmutableArray<IValidatorSource>.Empty );
                }

                var aspectResult = aspectBuilder.ToResult();

                if ( !aspectResult.Success )
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
                        validationRunner.RunDeclarationValidators( compilationModelRevision, CompilationModelVersion.Current, diagnosticSink );

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