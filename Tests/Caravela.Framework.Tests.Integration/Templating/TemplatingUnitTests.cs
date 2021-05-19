// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Templating
{
    public class TemplatingUnitTests : UnitTestBase
    {
        public TemplatingUnitTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [FromDirectory( @"Templating\Syntax" )]
        public Task Syntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\Pragma" )]
        public Task Pragma( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\LocalVariables" )]
        public Task LocalVariables( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\MagicKeywords" )]
        public Task MagicKeywords( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\Return" )]
        public Task Return( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\NamespaceExpansion" )]
        public Task NamespaceExpansion( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\Dynamic" )]
        public Task Dynamic( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"Templating\UnsupportedSyntax" )]
        public Task UnsupportedSyntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        protected override TestRunnerBase CreateTestRunner() => new TemplatingTestRunner( this.ServiceProvider, this.ProjectDirectory );
    }
}