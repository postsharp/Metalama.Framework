// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedUpdatableCollection<T> : UpdatableMemberCollection<T>
    where T : class, IMemberOrNamedType
{
    private ImmutableDictionary<string, UpdatableMemberRefArray<T>>? _byNameDictionary;

    protected NonUniquelyNamedUpdatableCollection( CompilationModel compilation, Ref<INamespaceOrNamedType> declaringNamespaceOrType ) : base( compilation, declaringNamespaceOrType ) { }

    private ImmutableDictionary<string, UpdatableMemberRefArray<T>> GetInitializedByNameDictionary()
        => this._byNameDictionary ??= ImmutableDictionary<string, UpdatableMemberRefArray<T>>.Empty.WithComparers( StringComparer.Ordinal );

    protected bool IsVisible( ISymbol symbol )
    {
        return this.Compilation.Project.ClassificationService?.GetExecutionScope( symbol ) != ExecutionScope.CompileTime;
    }

    public override ImmutableArray<MemberRef<T>> OfName( string name )
    {
        var byName = this.GetInitializedByNameDictionary();

        if ( byName.TryGetValue( name, out var updatableArray ) )
        {
            return updatableArray.Array;
        }
        else if ( !this.IsComplete )
        {
            var members = new UpdatableMemberRefArray<T>( this.GetMemberRefsOfName( name ), this.Compilation, this.MemberRefComparer );
            this._byNameDictionary = byName.SetItem( name, members );

            return members.Array;
        }
        else
        {
            return ImmutableArray<MemberRef<T>>.Empty;
        }
    }

    protected override void PopulateAllItems( Action<Ref<T>> action )
    {
        var byNameDictionary = this.GetInitializedByNameDictionary();
        var byNameDictionaryBuilder = byNameDictionary.ToBuilder();

        // Add all items that already been discovered in a name-specific operation.
        foreach ( var array in byNameDictionary.Values )
        {
            foreach ( var item in array.Array )
            {
                action( item.ToRef() );
            }
        }

        // Discover from source.
        foreach ( var memberRef in this.GetMemberRefs() )
        {
            // We intentionally look in the initial dictionary (not the builder). If there is no value for this name, it means
            // that the collection was not built for that name, and we need to create it now.
            if ( !byNameDictionary.ContainsKey( memberRef.Name ) )
            {
                action( memberRef.ToRef() );

                if ( !byNameDictionaryBuilder.TryGetValue( memberRef.Name, out var members ) )
                {
                    // This is the first time this method processes a member of that name.
                    members = new UpdatableMemberRefArray<T>( ImmutableArray.Create( memberRef ), this.Compilation, this.MemberRefComparer );
                    byNameDictionaryBuilder[memberRef.Name] = members;
                }
                else
                {
                    // The current method has already processed a member of that name, so we need to update the existing collection.
                    members.Add( memberRef );
                }
            }
        }

        this._byNameDictionary = byNameDictionaryBuilder.ToImmutable();
    }

    public void Add( MemberRef<T> member )
    {
        var byNameDictionary = this.GetInitializedByNameDictionary();

        if ( !byNameDictionary.TryGetValue( member.Name, out var members ) )
        {
            if ( !this.IsComplete )
            {
                // The collection has not been populated yet, so do it now, and add the new member.
                members = new UpdatableMemberRefArray<T>( this.GetMemberRefsOfName( member.Name ).Add( member ), this.Compilation, this.MemberRefComparer );
            }
            else
            {
                // The collection has been populated and there is no item of that name, so only add the member.
                members = new UpdatableMemberRefArray<T>( ImmutableArray.Create( member ), this.Compilation, this.MemberRefComparer );
            }

            this._byNameDictionary = byNameDictionary.SetItem( member.Name, members );
        }
        else
        {
            // The collection has been populated. Add the new member.
            if ( ReferenceEquals( members.ParentCompilation, this.Compilation ) )
            {
                members.Add( member );
            }
            else
            {
                // The object was created for another compilation, so we need to create a clone for ourselves.
                members = new UpdatableMemberRefArray<T>( members.Array.Add( member ), this.Compilation, this.MemberRefComparer );
                this._byNameDictionary = byNameDictionary.SetItem( member.Name, members );
            }
        }

        this.AddItem( member.ToRef() );
    }

    protected abstract IEqualityComparer<MemberRef<T>> MemberRefComparer { get; }

    // TODO: Verify why Remove is never called.
    // Resharper disable UnusedMember.Global

    public void Remove( MemberRef<T> member )
    {
        var byNameDictionary = this.GetInitializedByNameDictionary();

        if ( !byNameDictionary.TryGetValue( member.Name, out var members ) )
        {
            if ( !this.IsComplete )
            {
                // The collection has not been populated yet, so do it now, and add the new member.
                var sourceMembers = this.GetMemberRefsOfName( member.Name );

                var index = sourceMembers.IndexOf( member, this.MemberRefComparer );

                if ( index < 0 )
                {
                    throw new AssertionFailedException( $"The collection does not contain the item '{member}'." );
                }

                members = new UpdatableMemberRefArray<T>( sourceMembers.RemoveAt( index ), this.Compilation, this.MemberRefComparer );
            }
            else
            {
                throw new AssertionFailedException( $"The collection was populated, but it did not contain the item '{member}'." );
            }

            this._byNameDictionary = byNameDictionary.SetItem( member.Name, members );
        }
        else
        {
            // The collection has been populated. Remove the member.
            if ( ReferenceEquals( members.ParentCompilation, this.Compilation ) )
            {
                members.Remove( member );
            }
            else
            {
                // The object was created for another compilation, so we need to create a clone for ourselves.
                var index = members.Array.IndexOf( member, this.MemberRefComparer );

                if ( index < 0 )
                {
                    throw new AssertionFailedException( $"The collection does not contain the item '{member}'." );
                }

                members = new UpdatableMemberRefArray<T>( members.Array.RemoveAt( index ), this.Compilation, this.MemberRefComparer );
                this._byNameDictionary = byNameDictionary.SetItem( member.Name, members );
            }
        }

        this.RemoveItem( member.ToRef() );
    }

    // TODO: Return IEnumerable?
    protected abstract ImmutableArray<MemberRef<T>> GetMemberRefsOfName( string name );

    protected abstract ImmutableArray<MemberRef<T>> GetMemberRefs();
}