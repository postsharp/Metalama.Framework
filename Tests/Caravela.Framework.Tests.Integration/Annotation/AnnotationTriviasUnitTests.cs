// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    public class AnnotationTriviasUnitTests : AnnotationUnitTestsBase
    {
        public AnnotationTriviasUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\Declarations" )]
        public Task Declarations( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

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
        [FromDirectory( @"TestInputs\Highlighting\ForStatements" )]
        public Task ForStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Highlighting\WhileStatements" )]
        public Task WhileStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Highlighting\DoWhileStatements" )]
        public Task DoWhileStatements( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );
    }
}
