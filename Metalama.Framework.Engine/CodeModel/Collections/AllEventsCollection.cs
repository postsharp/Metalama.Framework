// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllEventsCollection : AllMembersCollection<IEvent>, IEventCollection
{
    public AllEventsCollection( NamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IEvent> GetMembers( INamedType namedType ) => namedType.Events;
}