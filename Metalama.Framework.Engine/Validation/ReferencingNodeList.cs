// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using System;
using System.Collections.ObjectModel;

namespace Metalama.Framework.Engine.Validation;

public class ReferencingNodeList : Collection<ReferencingNode>
{
    public ReferenceKinds ReferenceKinds { get; private set; }

    protected override void InsertItem( int index, ReferencingNode item )
    {
        this.ReferenceKinds |= item.ReferenceKind;
        base.InsertItem( index, item );
    }

    protected override void ClearItems() => throw new NotSupportedException();

    protected override void SetItem( int index, ReferencingNode item ) => throw new NotSupportedException();

    protected override void RemoveItem( int index ) => throw new NotSupportedException();
}