﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedTypeMemberUpdatableCollection<T> : UniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    // Private members in referenced assemblies are not included because they are also not included in the "ref assembly" and this
    // would cause inconsistent behaviors between design time and compile time.

    protected override ISymbol? GetMember( string name )
        => this.DeclaringTypeOrNamespace.GetMembers( name ).FirstOrDefault( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected override IEnumerable<ISymbol> GetMembers()
        => this.DeclaringTypeOrNamespace.GetMembers().Where( x => this.IsSymbolIncluded( x ) && SymbolValidator.Instance.Visit( x ) );

    protected UniquelyNamedTypeMemberUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base(
        compilation,
        declaringType ) { }
}