using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;

namespace Caravela.Framework.Impl
{
    internal class AspectBuilder<T> : DiagnosticList, IAspectBuilder<T>
        where T : class, ICodeElement
    {
        private readonly IImmutableList<IAdvice> _declarativeAdvices;
        private bool _skipped;

        public T TargetDeclaration { get; }

        ICodeElement IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        private readonly AdviceFactory _adviceFactory;

        public IAdviceFactory AdviceFactory => this._adviceFactory;

        public void SkipAspect() => this._skipped = true;

        public AspectBuilder( T targetDeclaration, IEnumerable<IAdvice> declarativeAdvices, AdviceFactory adviceFactory )
            : base( targetDeclaration.DiagnosticLocation )
        {
            this.TargetDeclaration = targetDeclaration;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._adviceFactory = adviceFactory;
        }

        internal AspectInstanceResult ToResult()
        {
            var success = this.ErrorCount == 0;
            return success && !this._skipped
                ? new(
                    success,
                    this.Diagnostics.ToImmutableArray(),
                    this._declarativeAdvices.AddRange( this._adviceFactory.Advices ),
                    Array.Empty<IAspectSource>() )
                : new(
                    success,
                    this.Diagnostics.ToImmutableArray(),
                    Array.Empty<IAdvice>(),
                    Array.Empty<IAspectSource>() );
        }
    }
}