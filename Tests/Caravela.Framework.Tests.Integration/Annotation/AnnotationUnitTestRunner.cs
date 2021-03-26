// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

            var testInputWithAddedTrivias = new TestInput( testInput.TestName, testInput.ProjectDirectory, testSourceWithAddedTrivias, testInput.TestSourcePath, testInput.TargetSource );

            return base.RunAsync( testInputWithAddedTrivias );
        }
    }
}
