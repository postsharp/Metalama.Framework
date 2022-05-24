﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedUpdatableCollection<T> : UpdatableMemberCollection<T>
    where T : class, IMemberOrNamedType
{
    private ImmutableDictionary<string, MemberRef<T>>? _dictionary;

    protected UniquelyNamedUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    private ImmutableDictionary<string, MemberRef<T>> GetInitializedDictionary()
        => this._dictionary ??= ImmutableDictionary<string, MemberRef<T>>.Empty.WithComparers( StringComparer.Ordinal );

    public override void Add( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( member.Name, out var existingMember ) )
        {
            if ( !existingMember.IsDefault )
            {
                // Duplicate key.
                throw new AssertionFailedException();
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
            if ( this.GetMember( member.Name ) != null )
            {
                // Duplicate key.
                throw new AssertionFailedException();
            }
            else
            {
                this._dictionary = dictionary.SetItem( member.Name, member );
            }
        }

        this.AddItem( member.ToRef() );
    }

    protected abstract ISymbol? GetMember( string name );

    protected abstract IEnumerable<ISymbol> GetMembers();

    public override void Remove( MemberRef<T> member )
    {
        var dictionary = this.GetInitializedDictionary();

        if ( dictionary.TryGetValue( member.Name, out var existingMember ) )
        {
            if ( existingMember.IsDefault )
            {
                // Missing key.
                throw new AssertionFailedException();
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
            throw new AssertionFailedException();
        }
        else
        {
            // The dictionary was not yet populated.
            if ( this.GetMember( member.Name ) == null )
            {
                // Duplicate key.
                throw new AssertionFailedException();
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
        var dictionaryBuilder = dictionary.ToBuilder();

        // Add items that have already been retrieved.
        foreach ( var item in dictionary )
        {
            action( item.Value.ToRef() );
        }

        // Add items discovered from source code.
        foreach ( var symbol in this.GetMembers() )
        {
            var memberRef = new MemberRef<T>( symbol, this.Compilation.RoslynCompilation );

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
            var symbol = this.GetMember( name );

            if ( symbol == null )
            {
                this._dictionary = dictionary.SetItem( name, default );

                return ImmutableArray<MemberRef<T>>.Empty;
            }
            else
            {
                var memberRef = new MemberRef<T>( symbol, this.Compilation.RoslynCompilation );
                this._dictionary = dictionary.SetItem( name, memberRef );

                return ImmutableArray.Create( memberRef );
            }
        }
    }
}