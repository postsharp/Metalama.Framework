// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal abstract class MemberCollection<TMember> : MemberOrNamedTypeCollection<TMember>
        where TMember : class, IMember
    {
        [Obfuscation( Exclude = true )]
        public INamedType DeclaringType { get; }

        protected MemberCollection( NamedType declaringType, UpdatableMemberCollection<TMember> sourceItems ) : base(
            declaringType,
            sourceItems )
        {
            this.DeclaringType = declaringType;
        }
    }
}