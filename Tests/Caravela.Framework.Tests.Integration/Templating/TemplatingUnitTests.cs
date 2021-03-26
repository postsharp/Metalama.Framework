// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Templating
{
    public class TemplatingUnitTests : TemplateUnitTestBase
    {
        public TemplatingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Syntax" )]
        public Task Syntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\LocalVariables" )]
        public Task LocalVariables( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Proceed" )]
        public Task Proceed( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Return" )]
        public Task Return( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\NamespaceExpansion" )]
        public Task NamespaceExpansion( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\UnsupportedSyntax" )]
        public async Task UnsupportedSyntax( string testName )
        {
            var testResult = await this.RunTestAsync( testName );
            Assert.False( testResult.Success );
            testResult.AssertContainsDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
