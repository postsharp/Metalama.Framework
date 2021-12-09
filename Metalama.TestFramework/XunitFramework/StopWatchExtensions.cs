// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics;

namespace Metalama.TestFramework.XunitFramework
{
    internal static class StopWatchExtensions
    {
        public static decimal GetSeconds( this Stopwatch stopwatch ) => (decimal) stopwatch.Elapsed.TotalSeconds;
    }
}