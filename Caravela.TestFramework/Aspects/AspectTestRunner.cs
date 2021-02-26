using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework.Aspects
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner
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

            result.Diagnostics.AddRange( diagnostics );

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
                result.ErrorMessage = "Unhandled exception: " + exception;
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

        private class AspectTestPipelineContext : IAspectPipelineContext, IBuildOptions
        {
            private readonly string _testName;
            private readonly TestResult _testResult;

            public AspectTestPipelineContext( string testName, CSharpCompilation compilation, TestResult testResult )
            {
                this.Compilation = compilation;
                this._testName = testName;
                this._testResult = testResult;
                this.ManifestResources = new List<ResourceDescription>();
            }

            public CSharpCompilation Compilation { get; }

            ImmutableArray<object> IAspectPipelineContext.Plugins => ImmutableArray<object>.Empty;

            public IList<ResourceDescription> ManifestResources { get; }

            CancellationToken IAspectPipelineContext.CancellationToken => CancellationToken.None;

            IBuildOptions IAspectPipelineContext.BuildOptions => this;

            void IAspectPipelineContext.ReportDiagnostic( Diagnostic diagnostic )
            {
                this._testResult.Diagnostics.Add( diagnostic );
            }

            public bool HandleExceptions => false;

            bool IBuildOptions.AttachDebugger => false;

            bool IBuildOptions.MapPdbToTransformedCode => true;

            public string? CompileTimeProjectDirectory => Path.Combine( Environment.CurrentDirectory, "compileTime", this._testName );

            public string? CrashReportDirectory => null;
        }
    }
}
