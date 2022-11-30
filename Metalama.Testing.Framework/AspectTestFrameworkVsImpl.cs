// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.Framework.XunitFramework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;

namespace Metalama.Testing.Framework
{
    [ExcludeFromCodeCoverage]
    internal class AspectTestFrameworkVsImpl : ITestFramework, ISourceInformationProvider
    {
        private readonly IMessageSink? _messageSink;

        public AspectTestFrameworkVsImpl( IMessageSink? messageSink )
        {
            this._messageSink = messageSink;
        }

        void IDisposable.Dispose() { }

        ISourceInformation ISourceInformationProvider.GetSourceInformation( ITestCase testCase ) => (ISourceInformation) testCase;

        ITestFrameworkDiscoverer ITestFramework.GetDiscoverer( IAssemblyInfo assembly ) => new TestDiscoverer( assembly, this._messageSink );

        ITestFrameworkExecutor ITestFramework.GetExecutor( AssemblyName assemblyName ) => new TestExecutor( assemblyName );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set { }
        }
    }
}