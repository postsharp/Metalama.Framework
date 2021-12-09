// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.TestFramework.Utilities
{
    internal static class DisposableExtensions
    {
        /// <summary>
        /// Removes <c>IAsyncDisposable</c> from the resulting type, so we can use <c>using</c> instead of <c>await using</c> without a warning.
        /// </summary>
        public static IDisposable IgnoreAsyncDisposable( this IDisposable disposable ) => disposable;
    }
}