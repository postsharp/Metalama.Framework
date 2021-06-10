// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal partial class AnnotationUnitTestRunner : BaseTestRunner
    {
        public AnnotationUnitTestRunner( IServiceProvider serviceProvider, string projectDirectory, IEnumerable<MetadataReference> metadataReferences ) 
            : base( serviceProvider, projectDirectory, metadataReferences ) { }

        public override TestResult RunTest( TestInput testInput )
        {
            var tree = CSharpSyntaxTree.ParseText( testInput.SourceCode );
            TriviaAdder triviaAdder = new();
            var testSourceRootWithAddedTrivia = triviaAdder.Visit( tree.GetRoot() );
            var testSourceWithAddedTrivia = testSourceRootWithAddedTrivia!.ToFullString();

            var testInputWithAddedTrivia = TestInput.FromSource( testInput.TestName, testSourceWithAddedTrivia );

            var result = base.RunTest( testInputWithAddedTrivia );

            if ( !result.Success )
            {
                return result;
            }

            var templateSyntaxRoot = result.TemplateDocument!.GetSyntaxRootAsync().Result!;
            var templateSemanticModel = result.TemplateDocument.GetSemanticModelAsync().Result!;

            DiagnosticList diagnostics = new();

            TemplateCompiler templateCompiler = new( this.ServiceProvider, result.InitialCompilation! );

            var templateCompilerSuccess = templateCompiler.TryAnnotate(
                templateSyntaxRoot,
                templateSemanticModel,
                diagnostics,
                CancellationToken.None,
                out var annotatedTemplateSyntax );

            if ( !templateCompilerSuccess )
            {
                result.Report( diagnostics );
                result.SetFailed( "TemplateCompiler.TryAnnotate failed." );

                return result;
            }

            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;

            return result;
        }

        public override void ExecuteAssertions( TestInput testInput, TestResult testResult, ITestOutputHelper logger )
        {
            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            Assert.Equal( testResult.TemplateDocument!.GetSyntaxRootAsync().Result!.ToString(), testResult.AnnotatedTemplateSyntax !.ToString() );
        }
    }
}