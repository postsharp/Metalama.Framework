// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    internal class AspectBuilder<T> : IAspectBuilder<T>, IAspectBuilderInternal, IAspectReceiverParent
        where T : class, IDeclaration
    {
        private readonly UserDiagnosticSink _diagnosticSink;
        private readonly AspectPipelineConfiguration _configuration;
        private AspectReceiverSelector<T>? _declarationSelector;
        private ImmutableArray<IAspectSource> _aspectSources = ImmutableArray<IAspectSource>.Empty;
        private ImmutableArray<IValidatorSource> _validatorSources = ImmutableArray<IValidatorSource>.Empty;

        public AspectBuilder(
            T target,
            UserDiagnosticSink diagnosticSink,
            AdviceFactory adviceFactory,
            AspectPipelineConfiguration configuration,
            IAspectInstance aspectInstance,
            CancellationToken cancellationToken )
        {
            this.Target = target;
            this._diagnosticSink = diagnosticSink;
            this._configuration = configuration;
            this.AspectInstance = aspectInstance;
            this.AdviceFactory = adviceFactory;
            this.CancellationToken = cancellationToken;
            this.AspectPredecessor = new AspectPredecessor( AspectPredecessorKind.ChildAspect, aspectInstance );
        }

        public IProject Project => this.Target.Compilation.Project;

        public IAspectInstance AspectInstance { get; }

        void IAspectOrValidatorSourceCollector.AddAspectSource( IAspectSource aspectSource )
        {
            this._aspectSources = this._aspectSources.Add( aspectSource );
        }

        void IAspectOrValidatorSourceCollector.AddValidatorSource( IValidatorSource validatorSource )
        {
            this._validatorSources = this._validatorSources.Add( validatorSource );
        }

        public AdviceFactory AdviceFactory { get; }

        public DisposeAction WithPredecessor( in AspectPredecessor predecessor )
        {
            var oldPredecessor = this.AspectPredecessor;
            this.AspectPredecessor = predecessor;

            return new DisposeAction( () => this.AspectPredecessor = oldPredecessor );
        }

        IDiagnosticAdder IAspectBuilderInternal.DiagnosticAdder => this._diagnosticSink;

        public ScopedDiagnosticSink Diagnostics => new( this._diagnosticSink, this.Target, this.Target );

        public T Target { get; }

        private AspectReceiverSelector<T> GetAspectReceiverSelector()
            => this._declarationSelector ??= new AspectReceiverSelector<T>( this.Target.ToTypedRef(), this, CompilationModelVersion.Current );

        public IAspectReceiver<TMember> With<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => this.GetAspectReceiverSelector().With( selector );

        IAspectReceiver<TMember> IAspectReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.GetAspectReceiverSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector )
            => this.GetAspectReceiverSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );

        IDeclaration IAspectLayerBuilder.Target => this.Target;

        public IAdviceFactory Advice => this.AdviceFactory;

        public void SkipAspect() => this.IsAspectSkipped = true;

        public bool IsAspectSkipped { get; private set; }

        public IAspectState? State
        {
            get => this.AspectInstance.State;
            set => ((IAspectInstanceInternal) this.AspectInstance).SetState( value );
        }

        public CancellationToken CancellationToken { get; }

        internal AspectInstanceResult ToResult()
        {
            var success = this._diagnosticSink.ErrorCount == 0;

            return success && !this.IsAspectSkipped
                ? new AspectInstanceResult(
                    this.AspectInstance,
                    success,
                    this._diagnosticSink.ToImmutable(),
                    this.AdviceFactory.Advices.ToImmutableArray(),
                    this._aspectSources,
                    this._validatorSources )
                : new AspectInstanceResult(
                    this.AspectInstance,
                    success,
                    this._diagnosticSink.ToImmutable(),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
        }

        public void SetAspectLayerBuildAction( string layerName, Action<IAspectLayerBuilder<T>> buildAction ) => throw new NotImplementedException();

        public bool VerifyEligibility( IEligibilityRule<T> rule )
        {
            var result = rule.GetEligibility( this.Target );

            if ( result == EligibleScenarios.None )
            {
                var justification = rule.GetIneligibilityJustification( EligibleScenarios.Aspect, new DescribedObject<T>( this.Target ) );

                this._diagnosticSink.Report(
                    GeneralDiagnosticDescriptors.AspectNotEligibleOnTarget.CreateRoslynDiagnostic(
                        this.Diagnostics.DefaultTargetLocation.GetDiagnosticLocation(),
                        (this.AspectInstance.AspectClass.ShortName, this.Target, justification!) ) );

                this.SkipAspect();

                return false;
            }
            else if ( result == EligibleScenarios.Inheritance )
            {
                // If inheritance is allowed, we return false without reporting any error.

                this.SkipAspect();

                return false;
            }
            else
            {
                return true;
            }
        }

        public AspectPredecessor AspectPredecessor { get; private set; }

        Type IAspectReceiverParent.Type => this.AspectInstance.AspectClass.Type;

        UserCodeInvoker IAspectReceiverParent.UserCodeInvoker => this._configuration.UserCodeInvoker;

        IServiceProvider IAspectReceiverParent.ServiceProvider => this._configuration.ServiceProvider;

        BoundAspectClassCollection IAspectReceiverParent.AspectClasses => this._configuration.BoundAspectClasses;

        public ReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod )
            => ((IValidatorDriverFactory) this.AspectInstance.AspectClass).GetReferenceValidatorDriver( validateMethod );

        public DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate )
            => ((IValidatorDriverFactory) this.AspectInstance.AspectClass).GetDeclarationValidatorDriver( validate );
    }
}