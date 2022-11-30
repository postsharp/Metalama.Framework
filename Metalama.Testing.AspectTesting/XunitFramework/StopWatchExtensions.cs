// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Testing.AspectTesting.XunitFramework
{
    internal static class StopWatchExtensions
    {
        public static decimal GetSeconds( this Stopwatch stopwatch ) => (decimal) stopwatch.Elapsed.TotalSeconds;
    }
}