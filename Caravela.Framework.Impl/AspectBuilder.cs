// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    internal class AspectBuilder<T> : IAspectBuilder<T>
        where T : class, ICodeElement
    {
        private readonly DiagnosticSink _diagnosticSink;
        private readonly IImmutableList<IAdvice> _declarativeAdvices;
        private readonly AdviceFactory _adviceFactory;
        private bool _skipped;

        public IDiagnosticSink Diagnostics => this._diagnosticSink;

        public T TargetDeclaration { get; }

        ICodeElement IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        public IAdviceFactory AdviceFactory => this._adviceFactory;

        public void SkipAspect() => this._skipped = true;

        public IDictionary<string, object?> Tags => this._adviceFactory.Tags;

        public AspectBuilder( T targetDeclaration, DiagnosticSink diagnosticSink, IEnumerable<IAdvice> declarativeAdvices, AdviceFactory adviceFactory )
        {
            this.TargetDeclaration = targetDeclaration;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._diagnosticSink = diagnosticSink;
            this._adviceFactory = adviceFactory;
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
    }
}