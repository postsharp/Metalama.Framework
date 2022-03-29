// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Framework.Engine.Utilities
{
    public static class Logger
    {
        public static void Initialize()
        {
            if ( DiagnosticsService.Initialize( ProcessUtilities.ProcessKind ) )
            {
                var processInfo = DiagnosticsService.Instance.GetLogger( "ProcessInfo" );

                processInfo.Info?.Log( $"Command line: {Environment.CommandLine}" );
                processInfo.Info?.Log( $"Process kind: {ProcessUtilities.ProcessKind}" );
                processInfo.Info?.Log( $"Version: {AssemblyMetadataReader.BuildId}" );
            }

            DesignTime = DiagnosticsService.Instance.DesignTime();
            Remoting = DiagnosticsService.Instance.Remoting();
            DesignTimeEntryPointManager = DiagnosticsService.Instance.GetLogger( "DesignTimeEntryPointManager" );
        }

        // The DesignTime logger is used before the service container is initialized, therefore we use the global instance.
        public static ILogger DesignTime { get; private set; } = NullLogger.Instance;

        public static ILogger Remoting { get; private set; } = NullLogger.Instance;

        public static ILogger DesignTimeEntryPointManager { get; private set; } = NullLogger.Instance;
    }
}