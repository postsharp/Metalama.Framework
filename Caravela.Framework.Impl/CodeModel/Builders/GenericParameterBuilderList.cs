// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class GenericParameterBuilderList : List<TypeParameterBuilder>, IGenericParameterList
    {
        IEnumerator<ITypeParameter> IEnumerable<ITypeParameter>.GetEnumerator() => this.GetEnumerator();

        ITypeParameter IReadOnlyList<ITypeParameter>.this[int index] => this[index];

        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<TypeParameterBuilder> AsBuilderList => this;
    }
}