// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class EmptyNamespaceCollection : INamespaceCollection
{
    public int Count => 0;

    public INamespace? OfName( string name ) => null;

#pragma warning disable CA1822
    public IEnumerable<INamespace> OfTypeDefinition( INamedType typeDefinition ) => Array.Empty<INamespace>();
#pragma warning restore CA1822

    public IEnumerator<INamespace> GetEnumerator() => ((IEnumerable<INamespace>) Array.Empty<INamespace>()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}