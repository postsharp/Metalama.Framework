// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class TypeUpdatableCollection : UniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    private bool IsIncluded( INamedTypeSymbol t )
        => (t.ContainingType != null || this.Compilation.PartialCompilation.Types.Contains( t )) &&
           this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly;

    protected override ISymbol? GetMember( string name )
        => this.DeclaringTypeOrNamespace.GetTypeMembers( name )
            .FirstOrDefault( this.IsIncluded );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetTypeMembers()
            .Where( this.IsIncluded );
}