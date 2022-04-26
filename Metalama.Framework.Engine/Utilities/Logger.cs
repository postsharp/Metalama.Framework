// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Framework.Engine.Utilities
{
    public static class Logger
    {
        /// <summary>
        /// Retrieved all loggers from the support services.
        /// </summary>
        /// <remarks>
        /// This method should only be called from the Support.Initialize method.
        /// </remarks>
        internal static void Initialize()
        {
            var loggerFactory = DiagnosticsService.GetRequiredService<ILoggerFactory>();
            var processInfo = loggerFactory.GetLogger( "ProcessInfo" );

            processInfo.Info?.Log( $"Command line: {Environment.CommandLine}" );
            processInfo.Info?.Log( $"Process kind: {ProcessUtilities.ProcessKind}" );
            processInfo.Info?.Log( $"Version: {AssemblyMetadataReader.BuildId}" );

            DesignTime = loggerFactory.DesignTime();
            Remoting = loggerFactory.Remoting();
            DesignTimeEntryPointManager = loggerFactory.GetLogger( "DesignTimeEntryPointManager" );
        }

        // The DesignTime logger is used before the service container is initialized, therefore we use the global instance.
        public static ILogger DesignTime { get; private set; } = NullLogger.Instance;

        public static ILogger Remoting { get; private set; } = NullLogger.Instance;

        public static ILogger DesignTimeEntryPointManager { get; private set; } = NullLogger.Instance;
    }
}