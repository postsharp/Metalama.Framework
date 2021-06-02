// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects.AdvisedCode;
using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceEvent : AdviceMember<IEvent>, IAdviceEvent
    {
        public AdviceEvent( IEvent underlying ) : base( underlying ) { }

        public IType EventType => this.Underlying.EventType;

        [Memo]
        public IAdviceMethod Adder => new AdviceMethod( this.Underlying.Adder );

        [Memo]
        public IAdviceMethod Remover => new AdviceMethod( this.Underlying.Remover );

        [Memo]
        public IAdviceMethod? Raiser => this.Underlying.Raiser != null ? new AdviceMethod( this.Underlying.Raiser ) : null;

        IMethod IEvent.Adder => this.Adder;

        IMethod IEvent.Remover => this.Remover;

        IMethod? IEvent.Raiser => this.Raiser;

        public IEventInvoker? BaseInvoker => this.Underlying.BaseInvoker;

        public IEventInvoker Invoker => this.Underlying.Invoker;

        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public EventInfo ToEventInfo() => this.Underlying.ToEventInfo();
    }
}