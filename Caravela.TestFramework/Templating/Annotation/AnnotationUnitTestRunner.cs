using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework.Templating.Annotation
{
    internal class AnnotationUnitTestRunner : TemplateTestRunnerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationUnitTestRunner"/> class.
        /// </summary>
        public AnnotationUnitTestRunner()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationUnitTestRunner"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public AnnotationUnitTestRunner( IEnumerable<CSharpSyntaxVisitor> testAnalyzers )
            : base( testAnalyzers )
        {
        }

        public override Task<TestResult> RunAsync( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.TestSource );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivias = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivias = testSourceRootWithAddedTrivias.ToFullString();

            var testInputWithAddedTrivias = new TestInput( testInput.TestName, testInput.ProjectDirectory, testSourceWithAddedTrivias, testInput.TestSourcePath, testInput.TargetSource );

            return base.RunAsync( testInputWithAddedTrivias );
        }
    }
}
