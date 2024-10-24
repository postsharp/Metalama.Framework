﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>, INamedTypeCollectionImpl
{
    public TypeUpdatableCollection( CompilationModel compilation, in Ref<INamespaceOrNamedType> declaringTypeOrNamespace ) : base(
        compilation,
        declaringTypeOrNamespace ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
    {
        if ( !base.IsSymbolIncluded( symbol ) )
        {
            return false;
        }

        if ( symbol.ContainingAssembly == this.Compilation.RoslynCompilation.Assembly )
        {
            // For types defined in the current assembly, we need to take partial compilations into account.

            return IsIncludedInPartialCompilation( (INamedTypeSymbol) symbol );

            bool IsIncludedInPartialCompilation( INamedTypeSymbol t )
            {
                return t switch
                {
                    { ContainingType: { } containingType } => IsIncludedInPartialCompilation( containingType ),
                    _ => this.Compilation.PartialCompilation.Types.Contains( t.OriginalDefinition )
                };
            }
        }
        else
        {
            return true;
        }
    }

    protected override IEqualityComparer<MemberRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override ImmutableArray<MemberRef<INamedType>> GetMemberRefsOfName( string name )
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceOrTypeSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetTypeMembers( name )
                    .Where( this.IsSymbolIncluded )
                    .Select( s => new MemberRef<INamedType>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespaceOrNamedType =>

                // TODO: should return initial members of the builder.
                ImmutableArray<MemberRef<INamedType>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected override ImmutableArray<MemberRef<INamedType>> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceOrTypeSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetTypeMembers()
                    .Where( this.IsSymbolIncluded )
                    .Select( s => new MemberRef<INamedType>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespaceOrNamedType =>

                // TODO: should return initial members of the builder.
                ImmutableArray<MemberRef<INamedType>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

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