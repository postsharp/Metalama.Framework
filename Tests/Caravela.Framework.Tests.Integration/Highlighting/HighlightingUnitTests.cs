// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    public class HighlightingUnitTests : HighlightingUnitTestsBase
    {
        public HighlightingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\Declarations" )]
        public Task Declarations( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\Identifiers" )]
        public Task Identifiers( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\MemberAccess" )]
        public Task MemberAccess( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\InvocationExpressions" )]
        public Task InvocationExpressions( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\IfStatements" )]
        public Task IfStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\ForEachStatements" )]
        public Task ForEachStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\ForStatements" )]
        public Task ForStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Formatting\Regressions" )]
        public Task Regression( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Formatting\WhileStatements" )]
        public Task WhileStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Formatting\DoWhileStatements" )]
        public Task DoWhileStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );
    }
}
