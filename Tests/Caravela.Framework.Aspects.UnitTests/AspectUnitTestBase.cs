using Caravela.TestFramework;
using Caravela.UnitTestFramework;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public abstract class AspectUnitTestBase
    {
        private readonly ITestOutputHelper _logger;

        public static string ProjectDirectory { get; } =
            Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Single( a => a.Key == "ProjectDirectory" ).Value!;

        public AspectUnitTestBase( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

        protected async Task<TestResult> RunPipelineAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( ProjectDirectory, testPath );

            var testSource = await File.ReadAllTextAsync( sourceAbsolutePath );
            var testRunner = new AspectTestRunner() { HandlesException = false };
            return await testRunner.Run( testPath, testSource );
        }

        protected async Task AssertTransformedSourceEqualAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( ProjectDirectory, testPath );

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
            var actualTransformedPath = Path.Combine( Path.GetDirectoryName( sourceAbsolutePath )!, Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".actual_transformed.txt" );

            var targetTextSpan = TestSyntaxHelper.FindRegionSpan( testResult.TransformedTargetSyntax, "Target" );
            testResult.AssertTransformedSourceSpanEqual( expectedTransformedSource, targetTextSpan, actualTransformedPath );
        }
    }
}
