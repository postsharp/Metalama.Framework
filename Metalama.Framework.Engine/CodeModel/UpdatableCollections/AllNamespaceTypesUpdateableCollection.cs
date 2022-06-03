// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

    protected override IEnumerable<ISymbol> GetMembers( string name )
        => ((INamespaceSymbol) this.DeclaringTypeOrNamespace).SelectManyRecursive( ns => ns.GetNamespaceMembers(), includeThis: true )
            .SelectMany(
                ns => ns.GetTypeMembers( name ).Where( t => this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly ) );

    protected override IEnumerable<ISymbol> GetMembers()
        => ((INamespaceSymbol) this.DeclaringTypeOrNamespace).SelectManyRecursive( ns => ns.GetNamespaceMembers(), includeThis: true )
            .SelectMany( ns => ns.GetTypeMembers().Where( t => this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly ) );
}