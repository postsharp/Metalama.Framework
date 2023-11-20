// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

#pragma warning disable SA1402

internal abstract class UpdatableDeclarationCollection<TDeclaration, TRef> : BaseDeclarationCollection, ILazy, ISourceDeclarationCollection<TDeclaration, TRef>
    where TDeclaration : class, IDeclaration
    where TRef : IRefImpl<TDeclaration>, IEquatable<TRef>
{
    private List<TRef>? _allItems;
    private volatile int _removeOperationsCount;

    protected UpdatableDeclarationCollection( CompilationModel compilation ) : base( compilation ) { }

    /// <summary>
    /// Gets a value indicating whether the construction has been populated for all names.
    /// </summary>
    public bool IsComplete { get; private set; }

    protected void EnsureComplete()
    {
        if ( this.IsComplete )
        {
            return;
        }

        lock ( this )
        {
            if ( this.IsComplete )
            {
                return;
            }

            this._allItems = [];

#if DEBUG
            this.PopulateAllItems(
                r =>
                {
                    if ( r.IsDefault )
                    {
                        throw new AssertionFailedException( "The reference is not initialized." );
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
    
    internal int Version { get; private set; }

    protected void AddItem( in TRef item )
    {
        if ( this.IsComplete )
        {
            this._allItems!.Add( item );
            this.Version++;
        }
    }

    protected void RemoveItem( in TRef item )
    {
        if ( this.IsComplete )
        {
            Interlocked.Increment( ref this._removeOperationsCount );
            this._allItems!.Remove( item );
            this.Version++;
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

    public ISourceDeclarationCollection<TDeclaration, TRef> Clone( CompilationModel compilation )
    {
        var clone = (UpdatableDeclarationCollection<TDeclaration, TRef>) this.MemberwiseClone();
        clone.Compilation = compilation;

        if ( this._allItems != null )
        {
            lock ( this )
            {
                clone._allItems = new List<TRef>( this._allItems.Count );
                clone._allItems.AddRange( this._allItems );
            }
        }

        return clone;
    }

    IEnumerator<TRef> IEnumerable<TRef>.GetEnumerator() => this.GetEnumerator();

    public Enumerator GetEnumerator() => new( this );

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public TRef this[ int index ]
    {
        get
        {
            this.EnsureComplete();

            return this._allItems![index];
        }
    }

    public struct Enumerator : IEnumerator<TRef>
    {
        private readonly UpdatableDeclarationCollection<TDeclaration, TRef> _parent;
        private readonly int _initialCount;
        private readonly int _initialRemoveOperationsCount;
        private int _index = -1;

        internal Enumerator( UpdatableDeclarationCollection<TDeclaration, TRef> parent )
        {
            this._parent = parent;

            // In case elements are added while iterating, we only return the items that were present when iteration started.
            this._initialCount = parent.Count;

            // In case elements are removed while iterating, we fail.
            this._initialRemoveOperationsCount = parent._removeOperationsCount;
        }

        public bool MoveNext()
        {
            if ( this._index + 1 < this._initialCount )
            {
                if ( this._parent._removeOperationsCount != this._initialRemoveOperationsCount )
                {
                    throw new InvalidOperationException( "An item was removed from the collection while an enumeration was in progress." );
                }

                this._index++;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset() => this._index = -1;

        public readonly TRef Current => this._parent[this._index];

        readonly object IEnumerator.Current => this.Current;

        public readonly void Dispose() { }
    }
}

internal abstract class UpdatableDeclarationCollection<TDeclaration> : UpdatableDeclarationCollection<TDeclaration, Ref<TDeclaration>>, ISourceDeclarationCollection<TDeclaration>
    where TDeclaration : class, IDeclaration
{
    protected UpdatableDeclarationCollection( CompilationModel compilation ) : base( compilation ) { }
}