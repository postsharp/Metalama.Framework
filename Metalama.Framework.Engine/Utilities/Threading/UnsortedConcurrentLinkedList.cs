// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Threading;

internal readonly struct UnsortedConcurrentLinkedList<T>
{
    private readonly ConcurrentLinkedList<T> _linkedList;

    public UnsortedConcurrentLinkedList()
    {
        this._linkedList = new ConcurrentLinkedList<T>();
    }

    public void Add( T item ) => this._linkedList.Add( item );

    public List<T> GetSortedItems( Comparison<T> comparison )
    {
        var list = this._linkedList.ToList();

        if ( list.Count > 1 )
        {
            list.Sort( comparison );
        }

        return list;
    }
}