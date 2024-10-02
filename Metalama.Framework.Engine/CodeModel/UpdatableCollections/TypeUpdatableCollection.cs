// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class TypeUpdatableCollection : NonUniquelyNamedUpdatableCollection<INamedType>
{
    public TypeUpdatableCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringTypeOrNamespace ) : base(
        compilation,
        declaringTypeOrNamespace ) { }

    public TypeUpdatableCollection( CompilationModel compilation ) : base(
        compilation,
        compilation.ToRef() ) { }

    protected override IEqualityComparer<IRef<INamedType>> MemberRefComparer => this.Compilation.CompilationContext.NamedTypeRefComparer;

    protected override DeclarationKind ItemsDeclarationKind => DeclarationKind.NamedType;
}