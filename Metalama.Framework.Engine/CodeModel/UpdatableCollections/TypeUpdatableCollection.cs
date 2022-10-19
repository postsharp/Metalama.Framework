// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class TypeUpdatableCollection : UniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => base.IsSymbolIncluded( symbol ) &&
           (symbol.ContainingType != null || this.Compilation.PartialCompilation.Types.Contains( symbol )) &&
           this.Compilation.SymbolClassifier.GetTemplatingScope( symbol ) != TemplatingScope.CompileTimeOnly;

    protected override ISymbol? GetMember( string name )
        => this.DeclaringTypeOrNamespace.GetTypeMembers( name )
            .FirstOrDefault( this.IsSymbolIncluded );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetTypeMembers()
            .Where( this.IsSymbolIncluded );
}