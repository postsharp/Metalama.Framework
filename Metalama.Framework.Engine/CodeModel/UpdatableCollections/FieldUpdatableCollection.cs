// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class FieldUpdatableCollection : UniquelyNamedUpdatableCollection<IField>
{
    public FieldUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType ) { }

    protected override DeclarationKind DeclarationKind => DeclarationKind.Field;
}