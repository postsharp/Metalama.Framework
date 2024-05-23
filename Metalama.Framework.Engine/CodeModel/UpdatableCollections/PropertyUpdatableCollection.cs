// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class PropertyUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IProperty>
{
    public PropertyUpdatableCollection( CompilationModel compilation, Ref<INamedType> declaringType ) : base( compilation, declaringType.As<INamespaceOrNamedType>() ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => symbol.Kind == SymbolKind.Property
           && base.IsSymbolIncluded( symbol ) && ((IPropertySymbol) symbol).Parameters.Length == 0;

    protected override IEqualityComparer<MemberRef<IProperty>> MemberRefComparer => this.Compilation.CompilationContext.PropertyRefComparer;
}