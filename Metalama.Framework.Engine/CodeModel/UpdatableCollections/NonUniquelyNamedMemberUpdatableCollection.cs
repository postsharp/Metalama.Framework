﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

    protected override IEnumerable<ISymbol> GetMembers( string name ) => this.DeclaringTypeOrNamespace.GetMembers( name ).Where( this.Predicate );

    protected override IEnumerable<ISymbol> GetMembers() => this.DeclaringTypeOrNamespace.GetMembers().Where( this.Predicate );

    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, INamespaceOrTypeSymbol declaringType ) : base(
        compilation,
        declaringType ) { }
}