// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class FieldUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IField>
{
    public FieldUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate => m => m.Kind == SymbolKind.Field && ((IFieldSymbol) m).AssociatedSymbol == null;
}