// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

#pragma warning disable SA1402

internal abstract class UpdatableDeclarationCollection<TDeclaration, TRef> : ILazy, IReadOnlyList<TRef>
    where TDeclaration : class, IDeclaration
    where TRef : IRefImpl<TDeclaration>, IEquatable<TRef>
{
    private List<TRef>? _allItems;

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
            this._allItems = new List<TRef>();

#if DEBUG
            this.PopulateAllItems(
                r =>
                {
                    if ( r.IsDefault )
                    {
                        throw new AssertionFailedException();
                    }

                    this._allItems.Add( r );
                } );
#else
            this.PopulateAllItems( this._allItems.Add );
#endif
            this.IsComplete = true;
        }
    }

    protected abstract void PopulateAllItems( Action<TRef> action );

    protected void AddItem( in TRef item )
    {
        if ( this.IsComplete )
        {
            this._allItems!.Add( item );
        }
    }

    protected void RemoveItem( in TRef item )
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

    public bool Contains( TRef item )
    {
        this.EnsureComplete();

        return this._allItems!.Any( i => i.Equals( item ) );
    }

    public UpdatableDeclarationCollection<TDeclaration, TRef> Clone( CompilationModel compilation )
    {
        var clone = (UpdatableDeclarationCollection<TDeclaration, TRef>) this.MemberwiseClone();
        clone.Compilation = compilation;

        if ( this._allItems != null )
        {
            clone._allItems = new List<TRef>( this._allItems.Count );
            clone._allItems.AddRange( this._allItems );
        }

        return clone;
    }

    public IEnumerator<TRef> GetEnumerator()
    {
        for ( var i = 0; i < this.Count; i++ )
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public TRef this[ int index ]
    {
        get
        {
            this.EnsureComplete();

            return this._allItems![index];
        }
    }
}

internal abstract class UpdatableDeclarationCollection<TDeclaration> : UpdatableDeclarationCollection<TDeclaration, Ref<TDeclaration>>
    where TDeclaration : class, IDeclaration
{
    protected UpdatableDeclarationCollection( CompilationModel compilation ) : base( compilation ) { }
}