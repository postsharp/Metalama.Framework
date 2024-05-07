// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Caching;

internal static class Pools
{
    public static ObjectPool<SemaphoreSlim> SemaphoreSlim { get; } = new( () => new SemaphoreSlim( 1 ) );
}