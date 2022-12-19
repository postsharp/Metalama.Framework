// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class PropertyUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IProperty>
{
    public PropertyUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => symbol.Kind == SymbolKind.Property
           && base.IsSymbolIncluded( symbol ) && ((IPropertySymbol) symbol).Parameters.Length == 0;
}