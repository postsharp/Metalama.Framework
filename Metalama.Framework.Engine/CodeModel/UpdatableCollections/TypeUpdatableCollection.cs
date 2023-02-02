// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : UniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override ISymbol? GetMember( string name )
        => this.DeclaringTypeOrNamespace.GetTypeMembers( name )
            .FirstOrDefault( this.IsSymbolIncluded );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetTypeMembers()
            .Where( this.IsSymbolIncluded );
}