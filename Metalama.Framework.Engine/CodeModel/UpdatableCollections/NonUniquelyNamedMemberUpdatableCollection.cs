// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedMemberUpdatableCollection<T> : NonUniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected override IEnumerable<ISymbol> GetSymbolsOfName( string name )
        => this.DeclaringTypeOrNamespace.GetMembers( name ).Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected override IEnumerable<ISymbol> GetSymbols()
        => this.DeclaringTypeOrNamespace.GetMembers().Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base(
        compilation,
        declaringType ) { }
}