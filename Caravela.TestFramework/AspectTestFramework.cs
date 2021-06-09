// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.XunitFramework;
using System;
using System.Reflection;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Implementation of a Xunit test framework.
    /// </summary>
    public class AspectTestFramework : ITestFramework, ISourceInformationProvider
    {
        void IDisposable.Dispose() { }

        ISourceInformation ISourceInformationProvider.GetSourceInformation( ITestCase testCase ) => (ISourceInformation) testCase;

        ITestFrameworkDiscoverer ITestFramework.GetDiscoverer( IAssemblyInfo assembly ) => new TestDiscoverer( assembly );

        ITestFrameworkExecutor ITestFramework.GetExecutor( AssemblyName assemblyName ) => new TestExecutor( assemblyName );

        ISourceInformationProvider ITestFramework.SourceInformationProvider
        {
            set { }
        }
    }
}