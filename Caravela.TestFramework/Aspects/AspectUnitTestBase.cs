using System.IO;
using System.Threading.Tasks;
using Caravela.UnitTestFramework;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework.Aspects
{
    public abstract class AspectUnitTestBase
    {
        private readonly ITestOutputHelper _logger;

        protected string ProjectDirectory { get; }

        public AspectUnitTestBase( ITestOutputHelper logger )
        {
            this._logger = logger;
            this.ProjectDirectory = TestEnvironment.GetProjectDirectory( this.GetType().Assembly );
        }

        protected async Task<TestResult> RunPipelineAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, testPath );

            var testSource = await File.ReadAllTextAsync( sourceAbsolutePath );
            var testRunner = new AspectTestRunner() { HandlesException = false };
            return await testRunner.Run( testPath, testSource );
        }

        protected async Task AssertTransformedSourceEqualAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, testPath );

            var testResult = await this.RunPipelineAsync( testPath );

            foreach ( var diagnostic in testResult.Diagnostics )
            {
                if ( diagnostic.Severity == DiagnosticSeverity.Error )
                {
                    this._logger.WriteLine( diagnostic.ToString() );
                }
            }

            Assert.True( testResult.Success, testResult.ErrorMessage );

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourceAbsolutePath )!, Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".transformed.txt" );
            var expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );
            var actualTransformedPath = Path.Combine(
                this.ProjectDirectory,
                @"obj\transformed",
                Path.GetDirectoryName( testPath ) ?? "",
                Path.GetFileNameWithoutExtension( testPath ) + ".transformed.txt" );

            var targetTextSpan = TestSyntaxHelper.FindRegionSpan( testResult.TransformedTargetSyntax, "Target" );
            testResult.AssertTransformedSourceSpanEqual( expectedTransformedSource, targetTextSpan, actualTransformedPath );
        }
    }
}
