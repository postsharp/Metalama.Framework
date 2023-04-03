// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    internal sealed class AspectBuilder<T> : IAspectBuilder<T>, IAspectBuilderInternal, IAspectReceiverParent
        where T : class, IDeclaration
    {
        private readonly AspectBuilderState _aspectBuilderState;
        private AspectReceiverSelector<T>? _declarationSelector;

        public AspectBuilder(
            T target,
            AspectBuilderState aspectBuilderState,
            AdviceFactory adviceFactory,
            AspectPredecessor? aspectPredecessor = null )
        {
            this.Target = target;
            this._aspectBuilderState = aspectBuilderState;
            this.AdviceFactory = adviceFactory;
            this.AspectPredecessor = aspectPredecessor ?? new AspectPredecessor( AspectPredecessorKind.ChildAspect, aspectBuilderState.AspectInstance );
            this.LicenseVerifier = this.ServiceProvider.GetService<LicenseVerifier>();
        }

        public IProject Project => this.Target.Compilation.Project;

        public IAspectInstance AspectInstance => this._aspectBuilderState.AspectInstance;

        void IAspectOrValidatorSourceCollector.AddAspectSource( IAspectSource aspectSource )
        {
            this._aspectBuilderState.AspectSources = this._aspectBuilderState.AspectSources.Add( aspectSource );
        }

        void IAspectOrValidatorSourceCollector.AddValidatorSource( IValidatorSource validatorSource )
        {
            this._aspectBuilderState.ValidatorSources = this._aspectBuilderState.ValidatorSources.Add( validatorSource );
        }

        public ProjectServiceProvider ServiceProvider => this._aspectBuilderState.ServiceProvider;

        public AdviceFactory AdviceFactory { get; }

        public DisposeAction WithPredecessor( in AspectPredecessor predecessor )
        {
            var oldPredecessor = this.AspectPredecessor;
            this.AspectPredecessor = predecessor;

            return new DisposeAction( () => this.AspectPredecessor = oldPredecessor );
        }

        IDiagnosticAdder IAspectBuilderInternal.DiagnosticAdder => this._aspectBuilderState.Diagnostics;

        public ScopedDiagnosticSink Diagnostics => new( this._aspectBuilderState.Diagnostics, this.Target, this.Target );

        public T Target { get; }

        [Memo]
        public IAspectReceiver<T> Outbound => this.GetAspectReceiverSelector().With( t => t );

        IDeclaration IAspectBuilder.Target => this.Target;

        private AspectReceiverSelector<T> GetAspectReceiverSelector()
            => this._declarationSelector ??= new AspectReceiverSelector<T>( this.Target.ToTypedRef(), this, CompilationModelVersion.Current );

        public IAspectReceiver<TMember> With<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => this.GetAspectReceiverSelector().With( selector );

        IAspectReceiver<TMember> IAspectReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.GetAspectReceiverSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector )
            => this.GetAspectReceiverSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );

        IAdviceFactory IAspectBuilder.Advice => this.AdviceFactory;

        public void SkipAspect() => this._aspectBuilderState.AdviceFactoryState.SkipAspect();

        public bool IsAspectSkipped => this._aspectBuilderState.IsAspectSkipped;

        public IAspectState? AspectState
        {
            get => this.AspectInstance.AspectState;
            set => ((IAspectInstanceInternal) this.AspectInstance).SetState( value );
        }

        public CancellationToken CancellationToken => this._aspectBuilderState.CancellationToken;

        public bool VerifyEligibility( IEligibilityRule<T> rule )
        {
            var result = rule.GetEligibility( this.Target );

            if ( result == EligibleScenarios.None )
            {
                var justification = rule.GetIneligibilityJustification( EligibleScenarios.Aspect, new DescribedObject<T>( this.Target ) );

                this._aspectBuilderState.Diagnostics.Report(
                    GeneralDiagnosticDescriptors.AspectNotEligibleOnTarget.CreateRoslynDiagnostic(
                        this.Diagnostics.DefaultTargetLocation.GetDiagnosticLocation(),
                        (this.AspectInstance.AspectClass.ShortName, this.Target.DeclarationKind, this.Target, justification!) ) );

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

        public string? Layer => this._aspectBuilderState.Layer;

        public IAspectBuilder<TNewTarget> WithTarget<TNewTarget>( TNewTarget newTarget )
            where TNewTarget : class, IDeclaration
        {
            if ( newTarget == this.Target )
            {
                return (IAspectBuilder<TNewTarget>) (object) this;
            }
            else
            {
                return new AspectBuilder<TNewTarget>( newTarget, this._aspectBuilderState, this.AdviceFactory, this.AspectPredecessor );
            }
        }

        public AspectPredecessor AspectPredecessor { get; private set; }

        Type IAspectReceiverParent.Type => this.AspectInstance.AspectClass.Type;

        UserCodeInvoker IAspectReceiverParent.UserCodeInvoker => this._aspectBuilderState.Configuration.UserCodeInvoker;

        ProjectServiceProvider IAspectReceiverParent.ServiceProvider => this._aspectBuilderState.Configuration.ServiceProvider;

        BoundAspectClassCollection IAspectReceiverParent.AspectClasses => this._aspectBuilderState.Configuration.BoundAspectClasses;

        public MethodBasedReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod )
            => ((IValidatorDriverFactory) this.AspectInstance.AspectClass).GetReferenceValidatorDriver( validateMethod );

        public ClassBasedReferenceValidatorDriver GetReferenceValidatorDriver( Type type )
            => ((IValidatorDriverFactory) this.AspectInstance.AspectClass).GetReferenceValidatorDriver( type );

        public DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate )
            => ((IValidatorDriverFactory) this.AspectInstance.AspectClass).GetDeclarationValidatorDriver( validate );

        public LicenseVerifier? LicenseVerifier { get; }
    }
}