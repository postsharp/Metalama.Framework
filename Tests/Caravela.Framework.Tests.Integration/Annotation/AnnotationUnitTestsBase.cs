// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    public abstract class AnnotationUnitTestsBase : UnitTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationUnitTestsBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        public AnnotationUnitTestsBase( ITestOutputHelper logger ) : base( logger )
        {
        }

        protected async Task<TestResult> RunAnnotationTestAsync( string relativeTestPath )
        {
            var testSourceAbsolutePath = Path.Combine( this.ProjectDirectory, relativeTestPath );
            var testRunner = new AnnotationUnitTestRunner();
            var testSource = await File.ReadAllTextAsync( testSourceAbsolutePath );
            var testResult = await testRunner.RunAsync( new TestInput( relativeTestPath, this.ProjectDirectory, testSource, relativeTestPath ) );

            this.WriteDiagnostics( testResult.Diagnostics );

            return testResult;
        }

        protected async Task AssertTriviasPreservedByAnnotator( string relativeTestPath )
        {
            var testResult = await this.RunAnnotationTestAsync( relativeTestPath );

            // There is an assertion in the TemplateTestRunnerBase.RunAsync checking that annotated syntax is equal to the original syntax.
            Assert.True( testResult.Success, testResult.ErrorMessage );
        }
    }
}
