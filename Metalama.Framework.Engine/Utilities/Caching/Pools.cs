// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Caching;

internal static class Pools
{
    /*
        | Method    | Mean     | Error    | StdDev   | Median   |
        |---------- |---------:|---------:|---------:|---------:|
        | NewObject | 51.00 ns | 1.712 ns | 5.047 ns | 49.15 ns |
        | Pooled    | 43.91 ns | 0.908 ns | 2.277 ns | 43.48 ns |
     */
    public static ObjectPool<SemaphoreSlim> SemaphoreSlim { get; } = new( () => new SemaphoreSlim( 1 ) );
}