// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedUpdatableCollection<T> : UpdatableMemberCollection<T>
    where T : class, IMemberOrNamedType
{
    private ImmutableDictionary<string, UpdatableMemberRefArray<T>>? _dictionary;

    protected NonUniquelyNamedUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    private ImmutableDictionary<string, UpdatableMemberRefArray<T>> GetInitializedDictionary()
        => this._dictionary ??= ImmutableDictionary<string, UpdatableMemberRefArray<T>>.Empty.WithComparers( StringComparer.Ordinal );

    public override ImmutableArray<MemberRef<T>> OfName( string name )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( name, out var updatableArray ) )
        {
            return updatableArray.Array;
        }
        else if ( !this.IsComplete )
        {
            var members = new UpdatableMemberRefArray<T>( this.GetMemberRefs( name ), this.Compilation );
            this._dictionary = dictionary.SetItem( name, members );

            return members.Array;
        }
        else
        {
            return ImmutableArray<MemberRef<T>>.Empty;
        }
    }

    protected override void PopulateAllItems( Action<Ref<T>> action )
    {
        var dictionary = this.GetInitializedDictionary();
        var dictionaryBuilder = dictionary.ToBuilder();

        // Add all items that already been discovered in a name-specific operation.
        foreach ( var array in dictionary.Values )
        {
            foreach ( var item in array.Array )
            {
                action( item.ToRef() );
            }
        }

        // Discover from source.
        foreach ( var symbol in this.GetMembers() )
        {
            // We intentionally look in the initial dictionary (not the builder). If there is no value for this name, it means
            // that the collection was not built for that name, and we need to create it now.
            if ( !dictionary.ContainsKey( symbol.Name ) )
            {
                var memberRef = new MemberRef<T>( symbol, this.Compilation.RoslynCompilation );

                action( memberRef.ToRef() );

                if ( !dictionaryBuilder.TryGetValue( memberRef.Name, out var members ) )
                {
                    // This is the first time this method processes a member of that name.
                    members = new UpdatableMemberRefArray<T>( ImmutableArray.Create( memberRef ), this.Compilation );
                    dictionaryBuilder[memberRef.Name] = members;
                }
                else
                {
                    // The current method has already processed a member of that name, so we need to update the existing collection.
                    members.Add( memberRef );
                }
            }
        }

        this._dictionary = dictionaryBuilder.ToImmutable();
    }

    public override void Add( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( !dictionary.TryGetValue( member.Name, out var members ) )
        {
            if ( !this.IsComplete )
            {
                // The collection has not been populated yet, so do it now, and add the new member.
                members = new UpdatableMemberRefArray<T>( this.GetMemberRefs( member.Name ).Add( member ), this.Compilation );
            }
            else
            {
                // The collection has been populated and there is no item of that name, so only add the member.
                members = new UpdatableMemberRefArray<T>( ImmutableArray.Create( member ), this.Compilation );
            }

            this._dictionary = dictionary.SetItem( member.Name, members );
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
                members = new UpdatableMemberRefArray<T>( members.Array.Add( member ), this.Compilation );
                this._dictionary = dictionary.SetItem( member.Name, members );
            }
        }

        this.AddItem( member.ToRef() );
    }

    public override void Remove( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( !dictionary.TryGetValue( member.Name, out var members ) )
        {
            if ( !this.IsComplete )
            {
                // The collection has not been populated yet, so do it now, and add the new member.
                var sourceMembers = this.GetMemberRefs( member.Name );

                var index = sourceMembers.IndexOf( member, MemberRefEqualityComparer<T>.Default );

                if ( index < 0 )
                {
                    // The source does not contain the expected symbol.
                    throw new AssertionFailedException();
                }

                members = new UpdatableMemberRefArray<T>( sourceMembers.RemoveAt( index ), this.Compilation );
            }
            else
            {
                // The collection was populated, but it did not contain the required item.
                throw new AssertionFailedException();
            }

            this._dictionary = dictionary.SetItem( member.Name, members );
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
                var index = members.Array.IndexOf( member, MemberRefEqualityComparer<T>.Default );

                if ( index < 0 )
                {
                    // The collection does not contain the expected symbol.
                    throw new AssertionFailedException();
                }

                members = new UpdatableMemberRefArray<T>( members.Array.RemoveAt( index ), this.Compilation );
                this._dictionary = dictionary.SetItem( member.Name, members );
            }
        }

        this.RemoveItem( member.ToRef() );
    }

    protected abstract IEnumerable<ISymbol> GetMembers( string name );

    protected abstract IEnumerable<ISymbol> GetMembers();

    private ImmutableArray<MemberRef<T>> GetMemberRefs( string name )
        => this.GetMembers( name )
            .Select( x => new MemberRef<T>( x, this.Compilation.RoslynCompilation ) )
            .ToImmutableArray();
}