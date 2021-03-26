// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    public abstract class HighlightingUnitTestsBase : UnitTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingUnitTestsBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        public HighlightingUnitTestsBase( ITestOutputHelper logger ) : base( logger )
        {
        }

        protected async Task<TestResult> RunHighlightingTestAsync( string relativeTestPath )
        {
            var testSourceAbsolutePath = Path.Combine( this.ProjectDirectory, relativeTestPath );
            var testRunner = new HighlightingTestRunner();
            var testSource = await File.ReadAllTextAsync( testSourceAbsolutePath );
            var testResult = await testRunner.RunAsync( new TestInput( relativeTestPath, this.ProjectDirectory, testSource, relativeTestPath, null ) );

            this.WriteDiagnostics( testResult.Diagnostics );

            return testResult;
        }

        protected async Task AssertHighlightedSourceEqualAsync( string relativeTestPath )
        {
            var testResult = await this.RunHighlightingTestAsync( relativeTestPath );

            Assert.True( testResult.Success, testResult.ErrorMessage );

            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, relativeTestPath );
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
