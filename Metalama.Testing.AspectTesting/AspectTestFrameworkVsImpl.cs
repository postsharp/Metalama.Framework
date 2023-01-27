// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting.XunitFramework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;

namespace Metalama.Testing.AspectTesting
{
    [ExcludeFromCodeCoverage]
    internal sealed class AspectTestFrameworkVsImpl : ITestFramework, ISourceInformationProvider
    {
        private readonly GlobalServiceProvider _serviceProvider;
        private readonly IMessageSink? _messageSink;

        public AspectTestFrameworkVsImpl( GlobalServiceProvider serviceProvider, IMessageSink? messageSink )
        {
            this._serviceProvider = serviceProvider;
            this._messageSink = messageSink;
        }

        void IDisposable.Dispose() { }

        ISourceInformation ISourceInformationProvider.GetSourceInformation( ITestCase testCase ) => (ISourceInformation) testCase;

        ITestFrameworkDiscoverer ITestFramework.GetDiscoverer( IAssemblyInfo assembly )
            => new TestDiscoverer( this._serviceProvider, assembly, this._messageSink );

        ITestFrameworkExecutor ITestFramework.GetExecutor( AssemblyName assemblyName ) => new TestExecutor( this._serviceProvider, assemblyName );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set { }
        }
    }
}