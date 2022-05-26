// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal abstract class MemberCollection<TMember> : MemberOrNamedTypeCollection<TMember>
        where TMember : class, IMember
    {
        public INamedType DeclaringType { get; }

        protected MemberCollection( INamedType declaringType, UpdatableMemberCollection<TMember> sourceItems ) : base(
            declaringType,
            sourceItems )
        {
            this.DeclaringType = declaringType;
        }
    }
}