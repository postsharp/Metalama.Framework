// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class AllNamespaceTypesUpdateableCollection : NonUniquelyNamedUpdatableCollection<INamedType>
{
    public AllNamespaceTypesUpdateableCollection( CompilationModel compilation, INamespaceSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override IEnumerable<ISymbol> GetSymbols( string name )
        => ((INamespaceSymbol) this.DeclaringTypeOrNamespace).SelectManyRecursive( ns => ns.GetNamespaceMembers(), includeThis: true )
            .SelectMany(
                ns => ns.GetTypeMembers( name )
                    .Where( t => this.Compilation.CompilationServices.SymbolClassifier.GetTemplatingScope( t ).GetExpressionExecutionScope() != TemplatingScope.CompileTimeOnly ) );

    protected override IEnumerable<ISymbol> GetSymbols()
        => ((INamespaceSymbol) this.DeclaringTypeOrNamespace).SelectManyRecursive( ns => ns.GetNamespaceMembers(), includeThis: true )
            .SelectMany(
                ns => ns.GetTypeMembers()
                    .Where( t => this.Compilation.CompilationServices.SymbolClassifier.GetTemplatingScope( t ).GetExpressionExecutionScope() != TemplatingScope.CompileTimeOnly ) );
}