// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    public class HighlightingUnitTests : UnitTestBase
    {
        public HighlightingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"Formatting" )]
        public Task All( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        protected override TestRunnerBase CreateTestRunner() => new HighlightingTestRunner( this.ProjectDirectory );

        protected async Task AssertHighlightedSourceEqualAsync( string relativeTestPath )
        {
            var testResult = await this.GetTestResultAsync( relativeTestPath );

            Assert.True( testResult.Success, testResult.ErrorMessage );

            var sourceAbsolutePath = Path.Combine( this.TestInputsDirectory, relativeTestPath );
            var expectedHighlightedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".highlighted.html" );
            var expectedHighlightedSource = await File.ReadAllTextAsync( expectedHighlightedPath );

            var actualHighlightedPath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "highlighted",
                Path.GetDirectoryName( relativeTestPath ) ?? "",
                Path.GetFileNameWithoutExtension( relativeTestPath ) + ".highlighted.html" );
            var actualHighlightedSource = await File.ReadAllTextAsync( actualHighlightedPath );

            Assert.Equal( expectedHighlightedSource, actualHighlightedSource );
        }
    }
}
