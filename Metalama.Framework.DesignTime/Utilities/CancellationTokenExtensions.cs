// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Framework.DesignTime.Utilities
{
    internal static class CancellationTokenExtensions
    {
        public static CancellationToken IgnoreIfDebugging( this CancellationToken cancellationToken ) => Debugger.IsAttached ? default : cancellationToken;
    }
}