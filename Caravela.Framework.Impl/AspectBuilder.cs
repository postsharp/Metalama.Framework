// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
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

namespace Caravela.Framework.Impl
{
    internal class AspectBuilder<T> : IAspectBuilder<T>
        where T : class, IDeclaration
    {
        private readonly UserDiagnosticSink _diagnosticSink;
        private readonly IImmutableList<IAdvice> _declarativeAdvices;
        private readonly AdviceFactory _adviceFactory;
        private bool _skipped;

        IProject IAspectBuilder.Project => throw new NotImplementedException();

        IReadOnlyList<IAspectMarkerInstance> IAspectBuilder.Markers => throw new NotImplementedException();

        IReadOnlyList<IAspectInstance> IAspectBuilder.OtherInstances => throw new NotImplementedException();

        public IDiagnosticSink Diagnostics => this._diagnosticSink;

        public T TargetDeclaration { get; }

        IDeclaration IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        public IAdviceFactory AdviceFactory => this._adviceFactory;

        public void SkipAspect() => this._skipped = true;

        public IDictionary<string, object?> Tags => this._adviceFactory.Tags;

        public CancellationToken CancellationToken { get; }

        public AspectBuilder(
            T targetDeclaration,
            UserDiagnosticSink diagnosticSink,
            IEnumerable<IAdvice> declarativeAdvices,
            AdviceFactory adviceFactory,
            CancellationToken cancellationToken )
        {
            this.TargetDeclaration = targetDeclaration;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._diagnosticSink = diagnosticSink;
            this._adviceFactory = adviceFactory;
            this.CancellationToken = cancellationToken;
        }

        internal AspectInstanceResult ToResult()
        {
            var success = this._diagnosticSink.ErrorCount == 0;

            return success && !this._skipped
                ? new AspectInstanceResult(
                    success,
                    this._diagnosticSink.ToImmutable(),
                    this._declarativeAdvices.ToImmutableArray().AddRange( this._adviceFactory.Advices ),
                    Array.Empty<IAspectSource>(),
                    this.Tags.ToImmutableDictionary() )
                : new AspectInstanceResult(
                    success,
                    this._diagnosticSink.ToImmutable(),
                    Array.Empty<IAdvice>(),
                    Array.Empty<IAspectSource>(),
                    ImmutableDictionary<string, object?>.Empty );
        }

#pragma warning disable 618 // Not implemented
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