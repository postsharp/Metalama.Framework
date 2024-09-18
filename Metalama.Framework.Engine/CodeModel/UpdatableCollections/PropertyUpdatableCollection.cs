// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class PropertyUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IProperty>
{
    public PropertyUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType.As<INamespaceOrNamedType>() ) { }

    protected override IEqualityComparer<IRef<IProperty>> MemberRefComparer => this.Compilation.CompilationContext.PropertyRefComparer;

    protected override DeclarationKind DeclarationKind => DeclarationKind.Property;
}