﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal sealed class AllEventsCollection : AllMembersCollection<IEvent>, IEventCollection
{
    public AllEventsCollection( NamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IEvent> GetMembers( INamedType namedType ) => namedType.Events;
}