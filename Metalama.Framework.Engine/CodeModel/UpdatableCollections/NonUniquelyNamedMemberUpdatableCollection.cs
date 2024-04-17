// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedMemberUpdatableCollection<T> : NonUniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected override IEnumerable<ISymbol> GetSymbolsOfName( string name )
        // TODO (TypeBuilder): Remove GetSymbol.
        => ((INamespaceOrTypeSymbol) this.DeclaringTypeOrNamespace.GetSymbol( this.Compilation.RoslynCompilation )).GetMembers( name ).Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected override IEnumerable<ISymbol> GetSymbols()
        // TODO (TypeBuilder): Remove GetSymbol.
        => ((INamespaceOrTypeSymbol) this.DeclaringTypeOrNamespace.GetSymbol( this.Compilation.RoslynCompilation )).GetMembers().Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, Ref<INamespaceOrNamedType> declaringType )
        : base( compilation, declaringType ) { }
}