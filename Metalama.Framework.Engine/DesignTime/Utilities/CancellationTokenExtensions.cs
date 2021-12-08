// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Utilities
{
    internal static class CancellationTokenExtensions
    {
        public static CancellationToken IgnoreIfDebugging( this CancellationToken cancellationToken ) => Debugger.IsAttached ? default : cancellationToken;
    }
}