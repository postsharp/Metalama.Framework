// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Tests.Integration.Templating
{
    public abstract class TemplatingTestRunnerBase
    {
        public virtual async Task<TestResult> RunAsync( TestInput testInput )
        {
            var testSource = testInput.TestSource;

            // Source.
            var project = this.CreateProject();
            var templateSourceText = SourceText.From( testSource.ToString(), encoding: Encoding.UTF8 );
            var testDocument = project.AddDocument( "Test.cs", templateSourceText );

            var compilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { (await testDocument.GetSyntaxTreeAsync())! },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions! );

            var result = new TestResult( project, testDocument, compilation );

            var diagnostics = compilation.GetDiagnostics();
            this.ReportDiagnostics( result, diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.ErrorMessage = "Initial diagnostics failed.";
                return result;
            }

            result.Success = true;
            return result;
        }

        /// <summary>
        /// Creates a new project that is used to compile the test source.
        /// </summary>
        /// <returns>A new project instance.</returns>
        protected virtual Microsoft.CodeAnalysis.Project CreateProject()
        {
            var referenceAssemblies = ReferenceAssemblyLocator.GetReferenceAssemblies();

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;
            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                    .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                    .AddMetadataReferences( referenceAssemblies.Select( f => MetadataReference.CreateFromFile( f ) ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( CompileTimeAttribute ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateSyntaxFactory ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TestTemplateAttribute ).Assembly.Location ) )
                ;
            return project;
        }

        /// <summary>
        /// Processes the diagnostics emitted during the test run.
        /// </summary>
        /// <param name="result">The current test result.</param>
        /// <param name="diagnostics">The diagnostics to report.</param>
        protected virtual void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            result.AddDiagnostics( diagnostics );
        }
    }
}
