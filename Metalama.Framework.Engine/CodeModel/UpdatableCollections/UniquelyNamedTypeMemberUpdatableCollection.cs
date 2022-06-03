// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedTypeMemberUpdatableCollection<T> : UniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected abstract Func<ISymbol, bool> Predicate { get; }

    protected override ISymbol? GetMember( string name ) => this.DeclaringTypeOrNamespace.GetMembers( name ).FirstOrDefault( this.Predicate );

    protected override IEnumerable<ISymbol> GetMembers() => this.DeclaringTypeOrNamespace.GetMembers().Where( this.Predicate );

    protected UniquelyNamedTypeMemberUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base(
        compilation,
        declaringType ) { }
}