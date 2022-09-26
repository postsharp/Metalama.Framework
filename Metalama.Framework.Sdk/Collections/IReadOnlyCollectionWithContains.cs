// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Collections;

internal interface IReadOnlyCollectionWithContains<T> : IReadOnlyCollection<T>
{
    bool Contains( T item );
}