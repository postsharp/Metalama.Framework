// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UpdatableDeclarationCollection<T> : ILazy, IReadOnlyList<Ref<T>>
    where T : class, IDeclaration
{
    private List<Ref<T>>? _allItems;

    protected UpdatableDeclarationCollection( CompilationModel compilation )
    {
        this.Compilation = compilation;
    }

    public CompilationModel Compilation { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the construction has been populated for all names.
    /// </summary>
    public bool IsComplete { get; private set; }

    protected void EnsureComplete()
    {
        if ( !this.IsComplete )
        {
            this._allItems = new List<Ref<T>>();
            this.PopulateAllItems( this._allItems.Add );
            this.IsComplete = true;
        }
    }

    protected abstract void PopulateAllItems( Action<Ref<T>> action );

    protected void AddItem( Ref<T> item )
    {
        if ( this.IsComplete )
        {
            this._allItems!.Add( item );
        }
    }

    protected void RemoveItem( Ref<T> item )
    {
        if ( this.IsComplete )
        {
            this._allItems!.Remove( item );
        }
    }

    public int Count
    {
        get
        {
            this.EnsureComplete();

            return this._allItems!.Count;
        }
    }

    public bool Contains( Ref<T> item )
    {
        this.EnsureComplete();

        return this._allItems!.Any( i => DeclarationRefEqualityComparer<Ref<T>>.Instance.Equals( i, item ) );
    }

    public UpdatableDeclarationCollection<T> Clone( CompilationModel compilation )
    {
        var clone = (UpdatableDeclarationCollection<T>) this.MemberwiseClone();
        clone.Compilation = compilation;

        return clone;
    }

    public IEnumerator<Ref<T>> GetEnumerator()
    {
        for ( var i = 0; i < this.Count; i++ )
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public Ref<T> this[ int index ]
    {
        get
        {
            this.EnsureComplete();

            return this._allItems![index];
        }
    }
}