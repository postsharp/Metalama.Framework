// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    public static class Logger
    {
        private static readonly object _initializeSync = new();
        private static ILoggerFactory? _loggerFactory;

        public static ILoggerFactory LoggerFactory => _loggerFactory ?? throw new InvalidOperationException( "Logger.Initialize has not been called." );

        /// <summary>
        /// Initializes all loggers from the support services.
        /// </summary>
        /// <remarks>
        /// This method should only be called from the MetalamaDiagnosticsService.Initialize method.
        /// </remarks>
        internal static void Initialize()
        {
            lock ( _initializeSync )
            {
                if ( _loggerFactory != null )
                {
                    return;
                }

                _loggerFactory = DiagnosticServiceFactory.ServiceProvider.GetLoggerFactory();
                var processInfo = _loggerFactory.GetLogger( "ProcessInfo" );

                processInfo.Info?.Log( $"Command line: {Environment.CommandLine}" );
                processInfo.Info?.Log( $"Process kind: {ProcessUtilities.ProcessKind}" );
                processInfo.Info?.Log( $"Version: {AssemblyMetadataReader.GetInstance( typeof(Logger).Assembly ).BuildId}" );

                DesignTime = _loggerFactory.DesignTime();
                Remoting = _loggerFactory.Remoting();
                DesignTimeEntryPointManager = _loggerFactory.GetLogger( "DesignTimeEntryPointManager" );
                Domain = _loggerFactory.GetLogger( "Domain" );
            }
        }

        // The DesignTime logger is used before the service container is initialized, therefore we use the global instance.
        public static ILogger DesignTime { get; private set; } = NullLogger.Instance;

        public static ILogger Remoting { get; private set; } = NullLogger.Instance;

        public static ILogger DesignTimeEntryPointManager { get; private set; } = NullLogger.Instance;

        public static ILogger Domain { get; private set; } = NullLogger.Instance;
    }
}