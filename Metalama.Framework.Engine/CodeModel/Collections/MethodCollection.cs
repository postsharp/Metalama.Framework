// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class MethodCollection : MethodBaseCollection<IMethod>, IMethodCollection
    {
        public MethodCollection( INamedType declaringType, MethodUpdatableCollection sourceItems ) : base( declaringType, sourceItems ) { }
    }
}