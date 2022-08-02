// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal abstract class MethodBaseCollection<T> : MemberCollection<T>
        where T : class, IMethodBase
    {
        protected MethodBaseCollection( NamedType declaringType, UpdatableMemberCollection<T> sourceItems ) : base(
            declaringType,
            sourceItems ) { }
    }
}