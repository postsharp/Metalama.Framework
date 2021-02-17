using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.UnitTestFramework;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public abstract class AspectUnitTestBase
    {
        private readonly ITestOutputHelper _logger;

        public AspectUnitTestBase( ITestOutputHelper logger )
        {
            this._logger = logger;
        }

       
        protected async Task RunTestAsync( string testName, string sourceAbsolutePath )
        {
            var expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourceAbsolutePath )!, Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".transformed.txt" );
            var actualTransformedPath = Path.Combine( Path.GetDirectoryName( sourceAbsolutePath )!, Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".actual_transformed.txt" );

            var testSource = await File.ReadAllTextAsync( sourceAbsolutePath );
            var expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );

            var testRunner = new AspectTestRunner() { HandlesException = false };
            var testResult = await testRunner.Run( testName, testSource );

            // We assume that the file must run without error. We would need another run method and more abstraction to
            // test for diagnostics.
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
            var targetTextSpan = TestSyntaxHelper.FindRegionSpan( testResult.TransformedTargetSyntax, "Target" );
            testResult.AssertTransformedSourceSpanEqual( expectedTransformedSource, targetTextSpan, actualTransformedPath );
        }

    
    }


}
