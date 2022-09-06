// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.TestFramework.XunitFramework;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.TestFramework
{
    /// <summary>
    /// The base class for the test suite that is automatically included in user projects.
    /// </summary>
    public abstract class DefaultTestSuite : TestSuite
    {
        protected DefaultTestSuite( ITestOutputHelper logger ) : base( logger ) { }

        protected override string GetDirectory( string callerMemberName )
        {
            var assemblyInfo = new ReflectionAssemblyInfo( this.GetType().Assembly );
            var discoverer = new TestDiscoverer( assemblyInfo );

            return discoverer.GetTestProjectProperties().ProjectDirectory;
        }
    }
}