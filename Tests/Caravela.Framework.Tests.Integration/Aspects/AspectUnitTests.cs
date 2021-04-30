// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Aspects
{
    public class AspectUnitTests : UnitTestBase
    {
        public AspectUnitTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [FromDirectory( @"Aspects\Order" )]
        public Task Order( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Aspects\Introductions\Methods" )]
        public Task Introductions( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Aspects\Overrides\Methods" )]
        public Task Overrides( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Aspects\Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Aspects\CodeModel" )]
        public Task CodeModel( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Aspects\Applying" )]
        public Task Applying( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        protected override TestRunnerBase CreateTestRunner() => new AspectTestRunner( this.ProjectDirectory );
    }
}