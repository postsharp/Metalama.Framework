// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class AttributeBuilderCollection : List<AttributeBuilder>, IAttributeCollection
    {
        IEnumerator<IAttribute> IEnumerable<IAttribute>.GetEnumerator() => this.GetEnumerator();

        public IEnumerable<IAttribute> OfAttributeType( INamedType type ) => throw new NotImplementedException();

        public IEnumerable<IAttribute> OfAttributeType( Type type ) => throw new NotImplementedException();
    }
}