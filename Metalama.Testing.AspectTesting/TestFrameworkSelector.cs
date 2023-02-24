// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
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
    /// Implementation of a Xunit test framework for Metalama that falls back to the default XUnit framework if Resharper or Rider is detected.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    public sealed class TestFrameworkSelector : ITestFramework, ISourceInformationProvider
    {
        static TestFrameworkSelector()
        {
            TestingServices.Initialize();
        }

        private readonly ITestFramework _implementation;

        public TestFrameworkSelector( IMessageSink messageSink )
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

            if ( ProcessUtilities.ProcessKind == ProcessKind.ResharperTestRunner )
            {
                messageSinkOrNull?.Trace( $"Resharper test runner detected. Using the legacy test framework." );

                this._implementation = new XunitTestFramework( messageSink );
            }
            else
            {
                messageSinkOrNull?.Trace( $"Resharper NOT detected. Using the customized test framework." );

                this._implementation = new AspectTestFramework( TestFrameworkServiceFactoryProvider.GetServiceProvider(), messageSinkOrNull );
            }
        }

        void IDisposable.Dispose() { }

        public ITestFrameworkDiscoverer GetDiscoverer( IAssemblyInfo assembly ) => this._implementation.GetDiscoverer( assembly );

        public ITestFrameworkExecutor GetExecutor( AssemblyName assemblyName ) => this._implementation.GetExecutor( assemblyName );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set => this._implementation.SourceInformationProvider = value;
        }

        ISourceInformation ISourceInformationProvider.GetSourceInformation( ITestCase testCase )
        {
            if ( this._implementation is ISourceInformationProvider sourceInformationProvider )
            {
                return sourceInformationProvider.GetSourceInformation( testCase );
            }
            else
            {
                return null!;
            }
        }
    }
}