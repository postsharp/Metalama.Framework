// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class MemberOrNamedTypeCollection<TMember> : MemberOrNamedTypeCollection<TMember, IRef<TMember>>
    where TMember : class, IMemberOrNamedType
{
    protected MemberOrNamedTypeCollection( IDeclaration containingDeclaration, ISourceDeclarationCollection<TMember, IRef<TMember>> sourceItems )
        : base( containingDeclaration, sourceItems ) { }
}