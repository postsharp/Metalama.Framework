// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class NonUniquelyNamedMemberUpdatableCollection<T> : NonUniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    protected NonUniquelyNamedMemberUpdatableCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringType )
        : base( compilation, declaringType ) { }
}