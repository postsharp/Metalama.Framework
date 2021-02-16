using Caravela.Framework.Advices;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class AspectBuilder<T> : IAspectBuilder<T>
        where T : class, ICodeElement
    {
        private readonly IImmutableList<IAdvice> _declarativeAdvices;
        
        public DiagnosticList Diagnostics { get; } = new DiagnosticList();

        public T TargetDeclaration { get; }

        ICodeElement IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        private readonly AdviceFactory _adviceFactory;

        public IAdviceFactory AdviceFactory => this._adviceFactory;

        public AspectBuilder( T targetDeclaration, IEnumerable<IAdvice> declarativeAdvices, AdviceFactory adviceFactory )
        {
            this.TargetDeclaration = targetDeclaration;
            this._declarativeAdvices = declarativeAdvices.ToImmutableArray();
            this._adviceFactory = adviceFactory;
        }

        internal AspectInstanceResult ToResult() =>
            new( 
                 this.Diagnostics.ToImmutableArray(), 
                this._declarativeAdvices.AddRange( this._adviceFactory.Advices ), 
                ImmutableList.Create<IAspectSource>() );


    }
}