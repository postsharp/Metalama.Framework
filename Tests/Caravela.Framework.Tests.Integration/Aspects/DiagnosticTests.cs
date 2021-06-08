// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Aspects
{
    public class DiagnosticTests : UnitTestBase
    {
        public DiagnosticTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [FromDirectory( "Aspects\\Diagnostics" )]
        public async Task Report( string path )
        {
            await this.AssertTransformedSourceEqualAsync( path );
        }

        [Theory]
        [FromDirectory( "Aspects\\Suppressions" )]
        public async Task Suppressions( string path )
        {
            await this.AssertTransformedSourceEqualAsync( path );
        }

        protected override TestRunnerBase CreateTestRunner( TestRunnerKind kind ) => new AspectTestRunner( this.ServiceProvider, this.ProjectDirectory );
    }
}