// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class FieldUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IField>
{
    public FieldUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType ) { }

    protected override IEqualityComparer<IRef<IField>?> MemberRefComparer => this.Compilation.CompilationContext.FieldRefComparer;

    protected override DeclarationKind DeclarationKind => DeclarationKind.Field;
}