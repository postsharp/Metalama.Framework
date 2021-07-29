// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedEvent : AdvisedMember<IEventInternal>, IAdvisedEvent
    {
        public AdvisedEvent( IEvent underlying ) : base( (IEventInternal) underlying ) { }

        public INamedType EventType => this.Underlying.EventType;

        public IMethod Signature => this.EventType.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IAdvisedMethod Adder => new AdvisedMethod( (IMethodInternal) this.Underlying.Adder );

        [Memo]
        public IAdvisedMethod Remover => new AdvisedMethod( (IMethodInternal) this.Underlying.Remover );

        [Memo]
        public IAdvisedMethod? Raiser => this.Underlying.Raiser != null ? new AdvisedMethod( (IMethodInternal) this.Underlying.Raiser ) : null;

        public IInvokerFactory<IEventInvoker> Invokers => this.Underlying.Invokers;

        public IEvent? OverriddenEvent => this.Underlying.OverriddenEvent;

        IMethod IEvent.Adder => this.Adder;

        IMethod IEvent.Remover => this.Remover;

        IMethod? IEvent.Raiser => this.Raiser;

        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public EventInfo ToEventInfo() => this.Underlying.ToEventInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );
    }
}