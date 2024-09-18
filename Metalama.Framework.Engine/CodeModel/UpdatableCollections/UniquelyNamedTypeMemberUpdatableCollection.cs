// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal abstract class UniquelyNamedTypeMemberUpdatableCollection<T> : UniquelyNamedUpdatableCollection<T>
    where T : class, IMemberOrNamedType
{
    // Private members in referenced assemblies are not included because they are also not included in the "ref assembly" and this
    // would cause inconsistent behaviors between design time and compile time.

    protected UniquelyNamedTypeMemberUpdatableCollection( CompilationModel compilation, IRef<INamespaceOrNamedType> declaringType ) : base(
        compilation,
        declaringType ) { }
}