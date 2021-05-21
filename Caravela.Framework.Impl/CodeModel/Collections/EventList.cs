// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class EventList : MemberList<IEvent, MemberRef<IEvent>>, IEventList
    {
        public EventList( NamedType containingElement, IEnumerable<MemberRef<IEvent>> sourceItems ) : base( containingElement, sourceItems ) { }
    }
}