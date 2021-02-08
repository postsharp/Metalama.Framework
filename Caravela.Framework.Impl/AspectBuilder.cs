﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class AspectBuilder<T> : IAspectBuilder<T>
        where T : class, ICodeElement
    {
        private readonly IImmutableList<AdviceInstance> _declarativeAdvices;

        public T TargetDeclaration { get; }

        ICodeElement IAspectBuilder.TargetDeclaration => this.TargetDeclaration;

        private readonly AdviceFactory _adviceFactory;

        public IAdviceFactory AdviceFactory => this._adviceFactory;

        public AspectBuilder( T targetDeclaration, IEnumerable<AdviceInstance> declarativeAdvices, AdviceFactory adviceFactory )
        {
            this.TargetDeclaration = targetDeclaration;
            this._declarativeAdvices = declarativeAdvices.ToImmutableList();
            this._adviceFactory = adviceFactory;
        }

        internal AspectInstanceResult ToResult() =>
            new ( ImmutableList.Create<Diagnostic>(), this._declarativeAdvices.AddRange( this._adviceFactory.Advices ), ImmutableList.Create<AspectInstance>() );
    }
}