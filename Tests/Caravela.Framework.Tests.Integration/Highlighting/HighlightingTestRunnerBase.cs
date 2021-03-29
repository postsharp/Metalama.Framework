// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    internal class HighlightingTestRunnerBase : TemplatingTestRunnerBase
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

            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;

            result.Success = true;

            return result;
        }
    }
}
