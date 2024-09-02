// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedMemberUpdatableCollection<T> : NonUniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected override ImmutableArray<MemberRef<T>> GetMemberRefsOfName( string name )
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamedTypeSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetMembers( name )
                    .Where( x => this.IsSymbolIncluded( x ) && this.Compilation.CompilationContext.SymbolValidator.IsValid( x ) )
                    .Select( s => new MemberRef<T>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespaceOrNamedType =>

                // TODO: should return initial members of the builder.
                ImmutableArray<MemberRef<T>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected override ImmutableArray<MemberRef<T>> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamedTypeSymbol symbol =>
                symbol.TranslateIfNecessary( this.Compilation.CompilationContext )
                    .GetMembers()
                    .Where( x => this.IsSymbolIncluded( x ) && this.Compilation.CompilationContext.SymbolValidator.IsValid( x ) )
                    .Select( s => new MemberRef<T>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespaceOrNamedType =>

                // TODO: should return initial members of the builder.
                ImmutableArray<MemberRef<T>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, in Ref<INamespaceOrNamedType> declaringType )
        : base( compilation, declaringType ) { }
}