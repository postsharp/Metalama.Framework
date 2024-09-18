// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class NamespaceUpdatableCollection : UniquelyNamedUpdatableCollection<INamespace>
{
    public NamespaceUpdatableCollection( CompilationModel compilation, IRef<INamespace> declaringNamespace ) : base(
        compilation,
        declaringNamespace.As<INamespaceOrNamedType>() ) { }

    protected override IEqualityComparer<IRef<INamespace>> MemberRefComparer => this.Compilation.CompilationContext.NamespaceRefComparer;

    protected override DeclarationKind DeclarationKind => DeclarationKind.Namespace;
}