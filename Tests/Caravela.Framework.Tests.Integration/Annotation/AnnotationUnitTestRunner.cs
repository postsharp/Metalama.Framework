// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    internal class AnnotationUnitTestRunner : TemplatingTestRunnerBase
    {

        public override async Task<TestResult> RunAsync( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.TestSource );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivias = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivias = testSourceRootWithAddedTrivias!.ToFullString();

            var testInputWithAddedTrivias = new TestInput( testInput.TestName, testInput.ProjectDirectory, testSourceWithAddedTrivias, testInput.TestSourcePath );

            var result = await base.RunAsync( testInputWithAddedTrivias );

            if ( !result.Success )
            {
                return result;
            }

            result.Success = false;

            var templateSyntaxRoot = (await result.TemplateDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await result.TemplateDocument.GetSemanticModelAsync())!;

            var templateCompiler = new TemplateCompiler();
            List<Diagnostic> diagnostics = new();
            var templateCompilerSuccess = templateCompiler.TryAnnotate( templateSyntaxRoot, templateSemanticModel, diagnostics, out var annotatedTemplateSyntax );

            this.ReportDiagnostics( result, diagnostics );

            if ( !templateCompilerSuccess )
            {
                result.ErrorMessage = "Template compiler failed.";
                return result;
            }

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax!.ToString() );
            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;

            result.Success = true;

            return result;
        }
    }
}
