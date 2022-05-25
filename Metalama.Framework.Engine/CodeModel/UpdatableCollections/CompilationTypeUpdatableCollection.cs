// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class CompilationTypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>
{
    public CompilationTypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override IEnumerable<ISymbol> GetMembers( string name )
    {
        return this.Compilation.PartialCompilation.Types
            .Where( t => t.Name == name && this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly );
    }

    protected override IEnumerable<ISymbol> GetMembers()
    {
        return this.Compilation.PartialCompilation.Types
            .Where( t => this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly );
    }
}