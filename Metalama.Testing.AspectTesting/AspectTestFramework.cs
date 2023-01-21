// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Testing.UnitTesting;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Implementation of a Xunit test framework for Metalama. Falls back to the default XUnit framework in Resharper or Rider.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    public class AspectTestFramework : ITestFramework
    {
        static AspectTestFramework()
        {
            TestingServices.Initialize();
        }

        private readonly ITestFramework _implementation;

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