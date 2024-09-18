// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class ConstructorUpdatableCollection : NonUniquelyNamedMemberUpdatableCollection<IConstructor>
{
    public ConstructorUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType.As<INamespaceOrNamedType>() ) { }

    // TODO: define implicit constructor
    protected override IEqualityComparer<IRef<IConstructor>> MemberRefComparer => this.Compilation.CompilationContext.ConstructorRefComparer;

    protected override DeclarationKind DeclarationKind => DeclarationKind.Constructor;
}