// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Collections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class AllMembersCollection<T> : AllMemberOrNamedTypesCollection<T, IMemberCollection<T>>, IMemberCollection<T>
    where T : class, IMember
{
    protected AllMembersCollection( INamedTypeImpl declaringType ) : base( declaringType ) { }

    INamedType IMemberCollection<T>.DeclaringType => this.DeclaringType;

    private INamedTypeImpl DeclaringType => (INamedTypeImpl) base.DeclaringNamedType;

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}