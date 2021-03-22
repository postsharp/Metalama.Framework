// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// The base class for aspect integration tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use <see cref="FromDirectoryAttribute"/> to execute one test method on many test inputs.
    /// This is useful to write only one test method per category of tests.
    /// </para>
    /// </remarks>
    public abstract class AspectUnitTestBase : UnitTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AspectUnitTestBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        protected AspectUnitTestBase( ITestOutputHelper logger ) : base( logger )
        {
        }

        /// <summary>
        /// Runs the aspect test with the given path of the source file.
        /// </summary>
        /// <param name="testPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The result of the test execution.</returns>
        protected async Task<TestResult> RunPipelineAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, testPath );

            var testSource = await File.ReadAllTextAsync( sourceAbsolutePath );
            var testRunner = new AspectTestRunner() { HandlesException = false };
            return await testRunner.RunAsync( testPath, testSource );
        }

        /// <summary>
        /// Runs the aspect test with the given path of the source file and asserts that the result of the code transformation matches the expected result.
        /// </summary>
        /// <param name="testPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The async task.</returns>
        protected async Task AssertTransformedSourceEqualAsync( string testPath )
        {
            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, testPath );

            var testResult = await this.RunPipelineAsync( testPath );

            foreach ( var diagnostic in testResult.Diagnostics )
            {
                if ( diagnostic.Severity == DiagnosticSeverity.Error )
                {
                    this.Logger.WriteLine( diagnostic.ToString() );
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
