using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework.Templating.Annotation
{
    internal class AnnotationUnitTestRunner : AnnotationUnitTestRunnerBase
    {
        public override Task<TestResult> RunAsync( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.TestSource );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivias = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivias = testSourceRootWithAddedTrivias!.ToFullString();

            var testInputWithAddedTrivias = new TestInput( testInput.TestName, testInput.ProjectDirectory, testSourceWithAddedTrivias, testInput.TestSourcePath, testInput.TargetSource );

            return base.RunAsync( testInputWithAddedTrivias );
        }
    }
}
