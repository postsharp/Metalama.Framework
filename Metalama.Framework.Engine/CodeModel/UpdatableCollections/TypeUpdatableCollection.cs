// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : UniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    // When the type is in the current assembly, we include only types that are in the partial compilation.
    protected override bool IsSymbolIncluded( ISymbol t )
        => base.IsSymbolIncluded( t )
           && (t.ContainingType != null || t.ContainingAssembly != this.Compilation.RoslynCompilation.Assembly
                                        || this.Compilation.PartialCompilation.Types.Contains( t ))
           && this.Compilation.CompilationContext.SymbolClassifier.GetTemplatingScope( t ).GetExpressionExecutionScope() != TemplatingScope.CompileTimeOnly;

    protected override ISymbol? GetMember( string name )
        => this.DeclaringTypeOrNamespace.GetTypeMembers( name )
            .FirstOrDefault( this.IsSymbolIncluded );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetTypeMembers()
            .Where( this.IsSymbolIncluded );
}