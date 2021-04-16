// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Templating
{
    public class TemplatingUnitTests : UnitTestBase
    {
        public TemplatingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"Templating\Syntax" )]
        public Task Syntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

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
        [FromDirectory( @"Templating\UnsupportedSyntax" )]
        public async Task UnsupportedSyntax( string testName )
        {
            var testResult = await this.GetTestResultAsync( testName );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id, StringComparison.Ordinal ) );
        }

        protected override TestRunnerBase CreateTestRunner() => new TemplatingTestRunner( this.ProjectDirectory );
    }
}
