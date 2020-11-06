using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    class AspectBuilder<T> : IAspectBuilder<T>
        where T : ICodeElement
    {
        private readonly List<AdviceInstance> _advices = new();

        public T TargetDeclaration { get; }
        ICodeElement IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        public IAdviceFactory AdviceFactory { get; }

        public AspectBuilder( T targetDeclaration, IEnumerable<AdviceInstance> declarativeAdvices, IAdviceFactory adviceFactory )
        {
            this.TargetDeclaration = targetDeclaration;
            this.AdviceFactory = adviceFactory;

            this._advices = declarativeAdvices.ToList();
        }

        public void AddAdvice<TAdviceElement>( IAdvice<TAdviceElement> advice ) where TAdviceElement : ICodeElement => this._advices.Add( new( advice ) );

        internal AspectInstanceResult ToResult() => new( ImmutableArray.Create<Diagnostic>(), this._advices.ToImmutableArray(), ImmutableArray.Create<AspectInstance>() );
    }
}