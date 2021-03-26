using System.Threading.Tasks;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    internal class AnnotationUnitTestRunner : AnnotationTestRunnerBase
    {
        public override Task<TestResult> RunAsync( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.TestSource );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivias = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivias = testSourceRootWithAddedTrivias!.ToFullString();

            var testInputWithAddedTrivias = new TestInput( testInput.TestName, testInput.ProjectDirectory, testSourceWithAddedTrivias, testInput.TestSourcePath );

            return base.RunAsync( testInputWithAddedTrivias );
        }
    }
}
