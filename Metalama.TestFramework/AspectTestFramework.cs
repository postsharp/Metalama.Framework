// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.TestFramework
{
    /// <summary>
    /// Implementation of a Xunit test framework for Metalama. Fall backs to the default XUnit framework in Resharper or Rider.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AspectTestFramework : ITestFramework
    {
        private readonly ITestFramework _implementation;

        public static void Initialize() { }

        public AspectTestFramework( IMessageSink messageSink )
        {
            // We disable logging by default because it creates too many log records.
            var messageSinkOrNull = string.IsNullOrEmpty( Environment.GetEnvironmentVariable( "LogMetalamaTestFramework" ) )
                ? null
                : messageSink;

            const string debugEnvironmentVariable = "DebugMetalamaTestFramework";

            if ( !string.IsNullOrEmpty( Environment.GetEnvironmentVariable( debugEnvironmentVariable ) ) )
            {
                messageSinkOrNull?.Trace( $"Environment variable '{debugEnvironmentVariable}' detected. Attaching debugger." );
                Debugger.Launch();
            }

            if ( Process.GetCurrentProcess().ProcessName.StartsWith( "ResharperTestRunner", StringComparison.OrdinalIgnoreCase ) )
            {
                messageSinkOrNull?.Trace( $"Resharper detected. Using the legacy test runner." );

                this._implementation = new XunitTestFramework( messageSink );
            }
            else
            {
                messageSinkOrNull?.Trace( $"Resharper NOT detected. Using the customized test runner." );

                this._implementation = new AspectTestFrameworkVsImpl( messageSinkOrNull );
            }
        }

        void IDisposable.Dispose() { }

        ITestFrameworkDiscoverer ITestFramework.GetDiscoverer( IAssemblyInfo assembly ) => this._implementation.GetDiscoverer( assembly );

        ITestFrameworkExecutor ITestFramework.GetExecutor( AssemblyName assemblyName ) => this._implementation.GetExecutor( assemblyName );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set => this._implementation.SourceInformationProvider = value;
        }
    }
}