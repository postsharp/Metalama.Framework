// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System;
using System.Diagnostics;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Exposes the <see cref="AttachDebugger"/> method.
    /// </summary>
    internal static class DesignTimeDebugger
    {
        private static readonly object _sync = new();
        private static volatile bool _attachDebuggerRequested;

        /// <summary>
        /// Attaches the debugger to the current process if requested.
        /// </summary>
        public static void AttachDebugger( IBuildOptions buildOptions )
        {
            if ( buildOptions.DesignTimeAttachDebugger && !_attachDebuggerRequested )
            {
                lock ( _sync )
                {
                    if ( !_attachDebuggerRequested )
                    {
                        // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                        // detaches. It makes a better debugging experience.
                        _attachDebuggerRequested = true;

                        if ( !Process.GetCurrentProcess().ProcessName.Equals( "devenv", StringComparison.OrdinalIgnoreCase ) &&
                             !Debugger.IsAttached )
                        {
                            Debugger.Launch();
                        }
                    }
                }
            }
        }
    }
}