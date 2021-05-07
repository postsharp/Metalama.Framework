// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilderList : List<AttributeBuilder>, IAttributeList
    {
        IEnumerator<IAttribute> IEnumerable<IAttribute>.GetEnumerator() => this.GetEnumerator();

        IAttribute IReadOnlyList<IAttribute>.this[ int index ] => this[index];
    }
}