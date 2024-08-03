// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class CompilationTypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>, INamedTypeCollectionImpl
{
    private readonly bool _includeNestedTypes;

    public CompilationTypeUpdatableCollection( CompilationModel compilation, in Ref<INamespaceOrNamedType> declaringType, bool includeNestedTypes ) : base(
        compilation,
        declaringType )
    {
        this._includeNestedTypes = includeNestedTypes;
    }

    protected override IEqualityComparer<MemberRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override ImmutableArray<MemberRef<INamedType>> GetMemberRefsOfName( string name )
    {
        // TODO: Optimize, what about introduced types?
        if ( this._includeNestedTypes )
        {
            throw new InvalidOperationException( "This method is not supported when the collection recursively includes nested types." );
        }

        return this.Compilation.PartialCompilation.Types
            .Where( t => t.Name == name && this.IsVisible( t ) )
            .Select( s => new MemberRef<INamedType>( s, this.Compilation.CompilationContext ) )
            .ToImmutableArray();
    }

    protected override ImmutableArray<MemberRef<INamedType>> GetMemberRefs()
    {
        // TODO: Optimize, what about introduced types?
        var topLevelTypes = this.Compilation.PartialCompilation.Types
            .Where( this.IsVisible );

        if ( !this._includeNestedTypes )
        {
            return
                topLevelTypes
                    .Select( s => new MemberRef<INamedType>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray();
        }
        else
        {
            var types = new List<ISymbol>();

            void ProcessType( INamedTypeSymbol type )
            {
                types.Add( type );

                foreach ( var nestedType in type.GetTypeMembers() )
                {
                    ProcessType( nestedType );
                }
            }

            foreach ( var type in topLevelTypes )
            {
                ProcessType( type );
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return
                types
                    .Select( s => new MemberRef<INamedType>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    public IEnumerable<MemberRef<INamedType>> OfTypeDefinition( INamedType typeDefinition )
    {
        var comparer = (DeclarationEqualityComparer) this.Compilation.Comparers.GetTypeComparer( TypeComparison.Default );

        // TODO: This should not use GetSymbol.
        return
            this.GetMemberRefs()
                .Where( t => comparer.Is( t, typeDefinition, ConversionKind.TypeDefinition ) )
                .Where(
                    t =>
                    {
                        var symbol = t.GetSymbol( this.Compilation.RoslynCompilation );

                        return symbol == null || this.IsSymbolIncluded( symbol );
                    } )
                .ToImmutableArray();
    }
}