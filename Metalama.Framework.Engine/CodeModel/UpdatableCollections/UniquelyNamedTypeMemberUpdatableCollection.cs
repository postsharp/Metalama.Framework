// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedTypeMemberUpdatableCollection<T> : UniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    // Private members in referenced assemblies are not included because they are also not included in the "ref assembly" and this
    // would cause inconsistent behaviors between design time and compile time.

    protected override MemberRef<T>? GetMemberRef( string name )
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceOrTypeSymbol symbol =>
                symbol.GetMembers( name )
                    .Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) )
                    .Select( s => (MemberRef<T>?) new MemberRef<T>( s, this.Compilation.CompilationContext ) )
                    .FirstOrDefault(),
            INamespaceOrNamedType namespaceOrNamedType =>

                // TODO: should return initial member of the builder.
                null,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected override IEnumerable<MemberRef<T>> GetMemberRefs()
        => this.DeclaringTypeOrNamespace.Target switch
        {
            INamespaceOrTypeSymbol symbol =>
                symbol.GetMembers()
                    .Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) )
                    .Select( s => new MemberRef<T>( s, this.Compilation.CompilationContext ) )
                    .ToImmutableArray(),
            INamespaceOrNamedType namespaceOrNamedType =>

                // TODO: should return initial members of the builder.
                ImmutableArray<MemberRef<T>>.Empty,
            _ => throw new AssertionFailedException( $"Unsupported {this.DeclaringTypeOrNamespace.Target}" )
        };

    protected UniquelyNamedTypeMemberUpdatableCollection( CompilationModel compilation, Ref<INamespaceOrNamedType> declaringType ) : base(
        compilation,
        declaringType ) { }
}