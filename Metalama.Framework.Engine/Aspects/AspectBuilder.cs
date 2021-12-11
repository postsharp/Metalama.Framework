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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects
{
    internal class AspectBuilder<T> : IAspectBuilder<T>, IAspectBuilderInternal
        where T : class, IDeclaration
    {
        private readonly UserDiagnosticSink _diagnosticSink;
        private readonly AspectPipelineConfiguration _configuration;
        private readonly IImmutableList<Advice> _declarativeAdvices;
        private bool _skipped;
        private AspectPredecessor _predecessor;

        public AspectBuilder(
            T target,
            UserDiagnosticSink diagnosticSink,
            IEnumerable<Advice> declarativeAdvices,
            AdviceFactory adviceFactory,
            AspectPipelineConfiguration configuration,
            IAspectInstance aspectInstance,
            CancellationToken cancellationToken )
        {
            this.Target = target;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._diagnosticSink = diagnosticSink;
            this._configuration = configuration;
            this.AspectInstance = aspectInstance;
            this.AdviceFactory = adviceFactory;
            this.CancellationToken = cancellationToken;
            this._predecessor = new AspectPredecessor( AspectPredecessorKind.ChildAspect, aspectInstance );
        }

        public IProject Project => this.Target.Compilation.Project;

        public IAspectInstance AspectInstance { get; }

        public ImmutableArray<IAspectSource> AspectSources { get; private set; } = ImmutableArray<IAspectSource>.Empty;
        public ImmutableArray<ValidatorSource> ValidatorSources { get; private set; } = ImmutableArray<ValidatorSource>.Empty;

        public void AddAspectSource( IAspectSource aspectSource )
        {
            this.AspectSources = this.AspectSources.Add( aspectSource );
        }

        public void AddValidatorSource( ValidatorSource validatorSource )
        {
            this.ValidatorSources = this.ValidatorSources.Add( validatorSource );
        }
        

        public AdviceFactory AdviceFactory { get; }

        public DisposeAction WithPredecessor( in AspectPredecessor predecessor )
        {
            var oldPredecessor = this._predecessor;
            this._predecessor = predecessor;

            return new DisposeAction( () => this._predecessor = oldPredecessor );
        }

        IDiagnosticAdder IAspectBuilderInternal.DiagnosticAdder => this._diagnosticSink;

        public IDiagnosticSink Diagnostics => this._diagnosticSink;

        public T Target { get; }

        public IDeclarationSelection<TMember> WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration
        {
            var executionContext = UserCodeExecutionContext.Current;

            return new DeclarationSelection<TMember>(
                this.Target.ToTypedRef(),
                this._predecessor,
                this.AddAspectSource,
                this.AddValidatorSource,
                ( compilation, diagnostics ) =>
                {
                    var translatedTarget = compilation.Factory.GetDeclaration( this.Target );

                    this._configuration.UserCodeInvoker.TryInvokeEnumerable(
                        () => selector( translatedTarget ),
                        executionContext.WithDiagnosticAdder( diagnostics ),
                        out var items );

                    return items ?? Enumerable.Empty<TMember>();
                },
                this._configuration.AspectClasses,
                this._configuration.ServiceProvider );
        }

        public IDeclarationSelection<T> WithTarget() => this.WithTargetMembers( declaration => new[]{ declaration } );
    
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
                    success,
                    this._diagnosticSink.ToImmutable(),
                    this._declarativeAdvices.ToImmutableArray().AddRange( this.AdviceFactory.Advices ),
                    this.AspectSources,
                    this.ValidatorSources )
                : new AspectInstanceResult(
                    success,
                    this._diagnosticSink.ToImmutable(),
                    ImmutableArray<Advice>.Empty, 
                    ImmutableArray<IAspectSource>.Empty, 
                    ImmutableArray<ValidatorSource>.Empty
                    );
        }

        public void SetAspectLayerBuildAction( string layerName, Action<IAspectLayerBuilder<T>> buildAction ) => throw new NotImplementedException();
    }
}