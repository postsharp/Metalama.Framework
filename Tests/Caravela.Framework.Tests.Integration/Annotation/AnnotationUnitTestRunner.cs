// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    internal class AnnotationUnitTestRunner : TestRunnerBase
    {
        public AnnotationUnitTestRunner( IServiceProvider serviceProvider, string projectDirectory ) : base( serviceProvider, projectDirectory ) { }

        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.TestSource );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivia = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivia = testSourceRootWithAddedTrivia!.ToFullString();

            var testInputWithAddedTrivia = new TestInput( testInput.TestName, testSourceWithAddedTrivia );

            var result = await base.RunTestAsync( testInputWithAddedTrivia );

            if ( !result.Success )
            {
                return result;
            }

            var templateSyntaxRoot = (await result.TemplateDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await result.TemplateDocument.GetSemanticModelAsync())!;

            DiagnosticList diagnostics = new();

            TemplateCompiler templateCompiler = new( this.ServiceProvider );

            var templateCompilerSuccess = templateCompiler.TryAnnotate(
                templateSyntaxRoot,
                templateSemanticModel,
                diagnostics,
                CancellationToken.None,
                out var annotatedTemplateSyntax );

            if ( !templateCompilerSuccess )
            {
                result.ReportDiagnostics( diagnostics );
                result.SetFailed( "TemplateCompiler.TryAnnotate failed." );

                return result;
            }

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax!.ToString() );
            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;

            return result;
        }
    }
}