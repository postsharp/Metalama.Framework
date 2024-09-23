// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class MemberCollection<TMember, TRef> : MemberOrNamedTypeCollection<TMember, TRef>
    where TMember : class, IMember
    where TRef : class, IRef<IDeclaration>
{
    public INamedType DeclaringType { get; }

    protected MemberCollection( INamedType declaringType, ISourceDeclarationCollection<TMember> sourceItems )
        : base( declaringType, sourceItems )
    {
        this.DeclaringType = declaringType;
    }
}