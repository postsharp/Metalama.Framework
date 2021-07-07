// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System;
using System.Reflection;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    internal class AspectTestFrameworkVsImpl : ITestFramework, ISourceInformationProvider
    {
        private readonly IMessageSink _messageSink;

        public AspectTestFrameworkVsImpl( IMessageSink messageSink )
        {
            this._messageSink = messageSink;
        }

        void IDisposable.Dispose() { }

        ISourceInformation ISourceInformationProvider.GetSourceInformation( ITestCase testCase ) => (ISourceInformation) testCase;

        ITestFrameworkDiscoverer ITestFramework.GetDiscoverer( IAssemblyInfo assembly ) => new TestDiscoverer( assembly, this._messageSink );

        ITestFrameworkExecutor ITestFramework.GetExecutor( AssemblyName assemblyName ) => new TestExecutor( assemblyName, this._messageSink );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set { }
        }
    }
}