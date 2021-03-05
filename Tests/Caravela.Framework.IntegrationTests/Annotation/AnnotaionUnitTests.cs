﻿using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Highlighting
{
    public class AnnotationTriviasUnitTests : TemplateUnitTestBase
    {
        public AnnotationTriviasUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\Declarations" )]
        public Task Declarations( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\LiteralExpressions" )]
        public Task LiteralExpressions( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\Identifiers" )]
        public Task Identifiers( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\MemberAccess" )]
        public Task MemberAccess( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\InvocationExpressions" )]
        public Task InvocationExpressions( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\IfStatements" )]
        public Task IfStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\ForEachStatements" )]
        public Task ForEachStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\CastExpressions" )]
        public Task CastExpressions( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\ForStatements" )]
        public Task ForStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\WhileStatements" )]
        public Task WhileStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\DoWhileStatements" )]
        public Task DoWhileStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );
    }
}
