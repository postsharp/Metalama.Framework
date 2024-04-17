// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class EmptyGenericParameterList : IGenericParameterList
{
    public int Count => 0;

    public INamedType DeclaringType { get; }

    public ITypeParameter this[ int index ] => throw new InvalidOperationException();

    public EmptyGenericParameterList( INamedType declaringType )
    {
        this.DeclaringType = declaringType;
    }

    public IEnumerator<ITypeParameter> GetEnumerator() => ((IEnumerable<ITypeParameter>) Array.Empty<ITypeParameter>()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}