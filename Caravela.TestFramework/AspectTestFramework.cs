// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Implementation of a Xunit test framework for Caravela. Fall backs to the default XUnit framework in Resharper or Rider.
    /// </summary>
    public class AspectTestFramework : ITestFramework
    {
        private readonly ITestFramework _implementation;

        public AspectTestFramework( IMessageSink messageSink )
        {
            const string debugEnvironmentVariable = "DebugCaravelaTestFramework";

            if ( !string.IsNullOrEmpty( Environment.GetEnvironmentVariable( debugEnvironmentVariable ) ) )
            {
                messageSink.Trace( $"Environment variable '{debugEnvironmentVariable}' detected. Attaching debugger." );
                Debugger.Launch();
            }

            if ( Process.GetCurrentProcess().ProcessName.StartsWith( "ResharperTestRunner", StringComparison.OrdinalIgnoreCase ) )
            {
                messageSink.Trace( $"Resharper detected. Using the legacy test runner." );

                this._implementation = new XunitTestFramework( messageSink );
            }
            else
            {
                messageSink.Trace( $"Resharper NOT detected. Using the customized test runner." );

                this._implementation = new AspectTestFrameworkVsImpl( messageSink );
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