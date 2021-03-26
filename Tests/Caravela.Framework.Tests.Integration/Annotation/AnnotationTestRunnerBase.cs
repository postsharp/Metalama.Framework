﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    internal abstract class AnnotationTestRunnerBase : TemplatingTestRunnerBase
    {
        public override async Task<TestResult> RunAsync( TestInput testInput )
        {
            var result = await base.RunAsync( testInput );

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
