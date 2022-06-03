// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class AttributeBuilderCollection : List<AttributeBuilder>, IAttributeCollection
    {
        IEnumerator<IAttribute> IEnumerable<IAttribute>.GetEnumerator() => this.GetEnumerator();
    }
}