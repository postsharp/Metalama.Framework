// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public partial class AspectTestRunner
    {
        /// <summary>
        /// Gets or sets a value indicating whether the test runner should handle or rethrow exceptions.
        /// </summary>
        public bool HandlesException { get; set; } = true;

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <param name="testName">The name of the test (usually the relative path to the test source file).</param>
        /// <param name="testSource">The content of the test source file.</param>
        /// <returns>The result of the test execution.</returns>
        public virtual async Task<TestResult> RunAsync( string testName, string testSource )
        {
            // Source.
            var project = this.CreateProject();
            var testDocument = project.AddDocument( "Test.cs", SourceText.From( testSource, encoding: Encoding.UTF8 ) );

            var initialCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { (await testDocument.GetSyntaxTreeAsync())! },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );

            var result = new TestResult( project, testDocument, initialCompilation );

            var diagnostics = initialCompilation.GetDiagnostics();

            result.AddDiagnostics( diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.ErrorMessage = "The initial compilation failed.";
                return result;
            }

            try
            {
                var context = new AspectTestPipelineContext( testName, initialCompilation, result );
                var pipeline = new CompileTimeAspectPipeline( context );
                if ( pipeline.TryExecute( out var resultCompilation ) )
                {
                    result.ResultCompilation = resultCompilation;
                    result.TransformedTargetSyntax = Formatter.Format( resultCompilation.SyntaxTrees.Single().GetRoot(), project.Solution.Workspace );
                    result.TransformedTargetSource = result.TransformedTargetSyntax.GetText();
                    result.Success = true;
                }
                else
                {
                    result.ErrorMessage = "The pipeline failed.";
                }
            }
            catch ( Exception exception ) when ( this.HandlesException )
            {
                result.ErrorMessage = "Unhandled exception: " + exception.Message;
                result.Exception = exception;
            }

            return result;
        }

        /// <summary>
        /// Creates a new project that is used to compile the test source.
        /// </summary>
        /// <returns>A new project instance.</returns>
        protected virtual Project CreateProject()
        {
            var referenceAssemblies = ReferenceAssemblyLocator.GetReferenceAssemblies();

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;
            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                    .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                    .AddMetadataReferences( referenceAssemblies.Select( f => MetadataReference.CreateFromFile( f ) ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( CompileTimeAttribute ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateSyntaxFactory ).Assembly.Location ) )
                ;
            return project;
        }

        /// <summary>
        /// Gets a list of analyzers to invoke on the test source.
        /// </summary>
        /// <returns>A list of C# syntax visitors.</returns>
        protected virtual IEnumerable<CSharpSyntaxVisitor> GetTestAnalyzers()
        {
            yield break;
        }
    }
}
