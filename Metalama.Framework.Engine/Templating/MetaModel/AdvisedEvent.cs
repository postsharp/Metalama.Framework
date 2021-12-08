// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedEvent : AdvisedMember<IEventImpl>, IAdvisedEvent
    {
        public AdvisedEvent( IEvent underlying ) : base( (IEventImpl) underlying ) { }

        public INamedType Type => this.Underlying.Type;

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IAdvisedMethod AddMethod => new AdvisedMethod( (IMethodImpl) this.Underlying.AddMethod );

        [Memo]
        public IAdvisedMethod RemoveMethod => new AdvisedMethod( (IMethodImpl) this.Underlying.RemoveMethod );

        [Memo]
        public IAdvisedMethod? RaiseMethod => this.Underlying.RaiseMethod != null ? new AdvisedMethod( (IMethodImpl) this.Underlying.RaiseMethod ) : null;

        public IInvokerFactory<IEventInvoker> Invokers => this.Underlying.Invokers;

        public IEvent? OverriddenEvent => this.Underlying.OverriddenEvent;

        IMethod IEvent.AddMethod => this.AddMethod;

        IMethod IEvent.RemoveMethod => this.RemoveMethod;

        IMethod? IEvent.RaiseMethod => this.RaiseMethod;

        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public EventInfo ToEventInfo() => this.Underlying.ToEventInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.Underlying.Accessors;

        IType IHasType.Type => this.Type;
    }
}