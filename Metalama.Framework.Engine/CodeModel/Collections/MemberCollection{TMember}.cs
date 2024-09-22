// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal abstract class MemberCollection<TMember> : MemberCollection<TMember, IRef<TMember>>
    where TMember : class, IMember
{
    protected MemberCollection( INamedType declaringType, ISourceDeclarationCollection<TMember, IRef<TMember>> sourceItems ) : base(
        declaringType,
        sourceItems ) { }
}