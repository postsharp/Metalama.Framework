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
            if ( Process.GetCurrentProcess().ProcessName.StartsWith( "ResharperTestRunner", StringComparison.OrdinalIgnoreCase ) )
            {
                this._implementation = new XunitTestFramework( messageSink );
            }
            else
            {
                this._implementation = new AspectTestFrameworkVsImpl();
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