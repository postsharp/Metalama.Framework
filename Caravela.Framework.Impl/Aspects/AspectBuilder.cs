// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.Aspects
{
    internal class AspectBuilder<T> : IAspectBuilder<T>, IAspectBuilderInternal
        where T : class, IDeclaration
    {
        private readonly UserDiagnosticSink _diagnosticSink;
        private readonly IImmutableList<Advice> _declarativeAdvices;
        private bool _skipped;

        public IProject Project => this.Target.Compilation.Project;

        public ImmutableArray<IAspectSource> AspectSources { get; private set; } = ImmutableArray<IAspectSource>.Empty;

        public void AddAspectSource( IAspectSource aspectSource ) => this.AspectSources = this.AspectSources.Add( aspectSource );

        public AdviceFactory AdviceFactory { get; }

        IReadOnlyList<IAspectInstance> IAspectLayerBuilder.UpstreamAspects => throw new NotImplementedException();

        IReadOnlyList<IAspectInstance> IAspectLayerBuilder.OtherInstances => throw new NotImplementedException();

        public IDiagnosticSink Diagnostics => this._diagnosticSink;

        public T Target { get; }

        [Obsolete( "Not implemented." )]
        public IDeclarationSelection<TMember> WithMembers<TMember>( Func<T, TMember> selector )
            where TMember : class, IDeclaration
            => throw new NotImplementedException();

        IDeclaration IAspectLayerBuilder.Target => this.Target;

        public IAdviceFactory Advices => this.AdviceFactory;

        public void SkipAspect() => this._skipped = true;

        public CancellationToken CancellationToken { get; }

        public AspectBuilder(
            T target,
            UserDiagnosticSink diagnosticSink,
            IEnumerable<Advice> declarativeAdvices,
            AdviceFactory adviceFactory,
            CancellationToken cancellationToken )
        {
            this.Target = target;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._diagnosticSink = diagnosticSink;
            this.AdviceFactory = adviceFactory;
            this.CancellationToken = cancellationToken;
        }

        internal AspectInstanceResult ToResult()
        {
            var success = this._diagnosticSink.ErrorCount == 0;

            return success && !this._skipped
                ? new AspectInstanceResult(
                    success,
                    this._diagnosticSink.ToImmutable(),
                    this._declarativeAdvices.ToImmutableArray().AddRange( this.AdviceFactory.Advices ),
                    this.AspectSources )
                : new AspectInstanceResult(
                    success,
                    this._diagnosticSink.ToImmutable(),
                    Array.Empty<Advice>(),
                    Array.Empty<IAspectSource>() );
        }

#pragma warning disable 618 // Not implemented
        void IAspectBuilder<T>.SetAspectLayerBuildAction( string layerName, Action<IAspectLayerBuilder<T>> buildAction ) => throw new NotImplementedException();

        void IValidatorAdder.AddTargetValidator<TTarget>( TTarget targetDeclaration, Action<ValidateReferenceContext<TTarget>> validator )
            => throw new NotImplementedException();

        void IValidatorAdder.AddReferenceValidator<TTarget, TConstraint>(
            TTarget targetDeclaration,
            IReadOnlyList<DeclarationReferenceKind> referenceKinds,
            IReadOnlyDictionary<string, string>? properties )
            => throw new NotImplementedException();
#pragma warning restore 618
    }
}