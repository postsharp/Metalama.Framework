// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class Logger
    {
        public static void Initialize()
        {
            DiagnosticsService.Initialize( DebuggingHelper.ProcessKind );
            DesignTime = DiagnosticsService.Instance.DesignTime();
        }

        // The DesignTime logger is used before the service container is initialized, therefore we use the global instance.
        public static ILogger DesignTime { get; private set; } = NullLogger.Instance;
    }
}