// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
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
        private readonly ImmutableArray<Advice> _declarativeAdvices;
        private bool _skipped;
        private AspectReceiverSelector<T>? _declarationSelector;

        public AspectBuilder(
            T target,
            UserDiagnosticSink diagnosticSink,
            ImmutableArray<Advice> declarativeAdvices,
            AdviceFactory adviceFactory,
            AspectPipelineConfiguration configuration,
            IAspectInstance aspectInstance,
            CancellationToken cancellationToken )
        {
            this.Target = target;
            this._declarativeAdvices = declarativeAdvices;
            this._diagnosticSink = diagnosticSink;
            this._configuration = configuration;
            this.AspectInstance = aspectInstance;
            this.AdviceFactory = adviceFactory;
            this.CancellationToken = cancellationToken;
            this.AspectPredecessor = new AspectPredecessor( AspectPredecessorKind.ChildAspect, aspectInstance );
        }

        public IProject Project => this.Target.Compilation.Project;

        public IAspectInstance AspectInstance { get; }

        public ImmutableArray<IAspectSource> AspectSources { get; private set; } = ImmutableArray<IAspectSource>.Empty;

        public ImmutableArray<IValidatorSource> ValidatorSources { get; private set; } = ImmutableArray<IValidatorSource>.Empty;

        void IAspectOrValidatorSourceCollector.AddAspectSource( IAspectSource aspectSource )
        {
            this.AspectSources = this.AspectSources.Add( aspectSource );
        }

        void IAspectOrValidatorSourceCollector.AddValidatorSource( IValidatorSource validatorSource )
        {
            this.ValidatorSources = this.ValidatorSources.Add( validatorSource );
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

        private AspectReceiverSelector<T> GetValidatorReceiverSelector()
            => this._declarationSelector ??= new AspectReceiverSelector<T>( this.Target.ToTypedRef(), this );

        public IAspectReceiver<TMember> WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
            => this.GetValidatorReceiverSelector().WithTargetMembers( selector );

        IValidatorReceiver<T> IValidatorReceiverSelector<T>.WithTarget() => this.WithTarget();

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
            => this.WithTargetMembers( selector );

        public IAspectReceiver<T> WithTarget() => this.GetValidatorReceiverSelector().WithTarget();

        public IValidatorReceiverSelector<T> AfterAllAspects() => this.GetValidatorReceiverSelector().AfterAllAspects();

        public IValidatorReceiverSelector<T> BeforeAnyAspect() => this.GetValidatorReceiverSelector().BeforeAnyAspect();

        IDeclaration IAspectLayerBuilder.Target => this.Target;

        public IAdviceFactory Advices => this.AdviceFactory;

        public void SkipAspect() => this._skipped = true;

        public IAspectState? State
        {
            get => this.AspectInstance.State;
            set => ((IAspectInstanceInternal) this.AspectInstance).SetState( value );
        }

        public CancellationToken CancellationToken { get; }

        internal AspectInstanceResult ToResult()
        {
            var success = this._diagnosticSink.ErrorCount == 0;

            return success && !this._skipped
                ? new AspectInstanceResult(
                    this.AspectInstance,
                    success,
                    this._diagnosticSink.ToImmutable(),
                    this._declarativeAdvices.AddRange( this.AdviceFactory.Advices ),
                    this.AspectSources,
                    this.ValidatorSources )
                : new AspectInstanceResult(
                    this.AspectInstance,
                    success,
                    this._diagnosticSink.ToImmutable(),
                    ImmutableArray<Advice>.Empty,
                    ImmutableArray<IAspectSource>.Empty,
                    ImmutableArray<IValidatorSource>.Empty );
        }

        public void SetAspectLayerBuildAction( string layerName, Action<IAspectLayerBuilder<T>> buildAction ) => throw new NotImplementedException();

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