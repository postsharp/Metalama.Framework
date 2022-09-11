﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal class PropertyUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IProperty>
{
    public PropertyUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate
        => m => m.Kind == SymbolKind.Property
                && ((IPropertySymbol) m).Parameters.Length == 0;
}