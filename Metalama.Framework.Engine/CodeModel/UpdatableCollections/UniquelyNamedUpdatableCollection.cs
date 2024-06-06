// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedUpdatableCollection<T> : UpdatableMemberCollection<T>
    where T : class, INamedDeclaration
{
    private ImmutableDictionary<string, MemberRef<T>>? _dictionary;

    protected UniquelyNamedUpdatableCollection( CompilationModel compilation, Ref<INamespaceOrNamedType> declaringType ) : base( compilation, declaringType ) { }

    private ImmutableDictionary<string, MemberRef<T>> GetInitializedDictionary()
        => this._dictionary ??= ImmutableDictionary<string, MemberRef<T>>.Empty.WithComparers( StringComparer.Ordinal, this.MemberRefComparer );

    protected abstract IEqualityComparer<MemberRef<T>> MemberRefComparer { get; }

    public void Add( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( member.Name, out var existingMember ) )
        {
            if ( !existingMember.IsDefault )
            {
                throw new AssertionFailedException( $"Duplicate item: '{member}'." );
            }
            else
            {
                // The dictionary was populated, but there was no member with that name yet.
                this._dictionary = dictionary.SetItem( member.Name, member );
            }
        }
        else if ( this.IsComplete )
        {
            // The dictionary was populated, but there was no member with that name yet.
            this._dictionary = dictionary.SetItem( member.Name, member );
        }
        else
        {
            // The dictionary was not yet populated.
            if ( !this.GetMemberRef( member.Name ).IsDefault )
            {
                throw new AssertionFailedException( $"Duplicate item: '{member}'." );
            }
            else
            {
                this._dictionary = dictionary.SetItem( member.Name, member );
            }
        }

        this.AddItem( member.ToRef() );
    }

    protected abstract MemberRef<T> GetMemberRef( string name );

    protected abstract IEnumerable<MemberRef<T>> GetMemberRefs();

    public void Remove( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( member.Name, out var existingMember ) )
        {
            if ( existingMember.IsDefault )
            {
                // Missing key.
                throw new AssertionFailedException( $"The collection does not contain '{member}'." );
            }
            else
            {
                // The dictionary was populated, and there was was a member with that name, so remove it.
                this._dictionary = dictionary.SetItem( member.Name, default );
            }
        }
        else if ( this.IsComplete )
        {
            // Missing key.
            throw new AssertionFailedException( $"The collection does not contain '{member}'." );
        }
        else
        {
            // The dictionary was not yet populated.
            if ( this.GetMemberRef( member.Name ).IsDefault )
            {
                // Missing key.
                throw new AssertionFailedException( $"Missing item: '{member}'." );
            }
            else
            {
                this._dictionary = dictionary.SetItem( member.Name, default );
            }
        }

        this.RemoveItem( member.ToRef() );
    }

    protected override void PopulateAllItems( Action<Ref<T>> action )
    {
        var dictionary = this.GetInitializedDictionary();

        // Add items that have already been retrieved.
        foreach ( var item in dictionary )
        {
            if ( !item.Value.IsDefault )
            {
                action( item.Value.ToRef() );
            }
        }

        var dictionaryBuilder = dictionary.ToBuilder();

        // Add items discovered from source code.
        foreach ( var memberRef in this.GetMemberRefs() )
        {
            if ( !dictionary.ContainsKey( memberRef.Name ) )
            {
                dictionaryBuilder[memberRef.Name] = memberRef;
                action( memberRef.ToRef() );
            }
            else
            {
                // This member has already been discovered by a previous call of this object.
            }
        }

        this._dictionary = dictionaryBuilder.ToImmutable();
    }

    public override ImmutableArray<MemberRef<T>> OfName( string name )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( name, out var member ) )
        {
            if ( member.IsDefault )
            {
                return ImmutableArray<MemberRef<T>>.Empty;
            }
            else
            {
                return ImmutableArray.Create( member );
            }
        }
        else
        {
            var memberRef = this.GetMemberRef( name );

            if ( memberRef.IsDefault )
            {
                this._dictionary = dictionary.SetItem( name, default );

                return ImmutableArray<MemberRef<T>>.Empty;
            }
            else
            {
                this._dictionary = dictionary.SetItem( name, memberRef );

                return ImmutableArray.Create( memberRef );
            }
        }
    }
}