// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

// TODO: caching (reuse old instances)
internal class MemberSubstitutedCollection<T> : ISourceMemberCollection<T>
    where T : class, IMemberOrNamedType
{
    public MemberSubstitutedCollection( ISourceMemberCollection<T> source, INamedTypeSymbol substitutedType )
    {
        this._source = source;
        this._substitutedType = substitutedType;
    }

    private readonly ISourceMemberCollection<T> _source;
    private readonly INamedTypeSymbol _substitutedType;
    
    public int Count => this._source.Count;

    public CompilationModel Compilation => this._source.Compilation;

    public INamespaceOrTypeSymbol DeclaringTypeOrNamespace => this._substitutedType;

    public IEnumerator<Ref<T>> GetEnumerator()
    {
        foreach ( var sourceRef in this._source )
        {
            yield return this.Substitute( sourceRef );
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public Ref<T> this[ int index ] => this.Substitute( this._source[index] );

    public ImmutableArray<MemberRef<T>> OfName( string name )
        => this._source.OfName( name ).SelectAsImmutableArray( memberRef => new MemberRef<T>( this.Substitute( memberRef.ToRef() ).As<IDeclaration>() ) );

    private Ref<T> Substitute( Ref<T> sourceRef )
        => SubstitutedMemberFactory.Substitute( sourceRef.GetTarget( this.Compilation ), this._substitutedType );

    ISourceDeclarationCollection<T, Ref<T>> ISourceDeclarationCollection<T, Ref<T>>.Clone( CompilationModel compilation )
        => new MemberSubstitutedCollection<T>(
            (ISourceMemberCollection<T>) this._source.Clone( compilation ),
            compilation.CompilationContext.SymbolTranslator.Translate( this._substitutedType ).AssertNotNull() );
}