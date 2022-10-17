// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedMemberUpdatableCollection<T> : NonUniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected abstract Func<ISymbol, bool> Predicate { get; }

    // Private members in referenced assemblies are not included because they are also not included in the "ref assembly" and this
    // would cause inconsistent behaviors between design time and compile time.
    private bool IsIncluded( ISymbol symbol ) => this.Predicate( symbol ) && !this.IsHidden( symbol );

    protected override IEnumerable<ISymbol> GetMembers( string name )
        => this.DeclaringTypeOrNamespace.GetMembers( name ).Where( x => this.IsIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetMembers().Where( x => this.IsIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base(
        compilation,
        declaringType ) { }
}