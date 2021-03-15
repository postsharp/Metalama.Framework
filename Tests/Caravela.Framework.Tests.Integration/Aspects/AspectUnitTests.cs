// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Aspects
{
    public class AspectUnitTests : AspectUnitTestBase
    {
        public AspectUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Aspects\Introductions\Methods" )]
        public Task Introductions( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Aspects\Overrides\Methods" )]
        public Task Overrides( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Aspects\Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );
    }
}
