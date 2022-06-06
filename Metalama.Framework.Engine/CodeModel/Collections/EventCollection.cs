// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class EventCollection : MemberCollection<IEvent>, IEventCollection
    {
        public EventCollection( INamedType declaringType, EventUpdatableCollection sourceItems ) : base( declaringType, sourceItems ) { }
    }
}