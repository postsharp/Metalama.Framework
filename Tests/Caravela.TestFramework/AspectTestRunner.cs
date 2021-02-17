using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using System.IO;

namespace Caravela.TestFramework
{
    public class AspectTestRunner
    {
        public bool HandlesException { get; set; } = true;
        
        public virtual async Task<TestResult> Run( string testName, string testSource )
        {

            testSource = CommonSnippets.CaravelaUsings + testSource;

            // Source.
            var project = this.CreateProject();
            var testDocument = project.AddDocument( "Test.cs", SourceText.From( testSource, encoding: Encoding.UTF8 ) );

            var result = new TestResult( testDocument );

            var initialCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { (await testDocument.GetSyntaxTreeAsync())! },
                project.MetadataReferences,
                (CSharpCompilationOptions?) project.CompilationOptions );
            
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
                CompileTimeAspectPipeline pipeline = new CompileTimeAspectPipeline( context );
                if ( pipeline.TryExecute( out var resultCompilation ) )
                {
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
                result.ErrorMessage = "Unhandled exception: " + exception.ToString();
            }
            
            return result;
        }

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

            public bool WriteUnhandledExceptionsToFile => true;
        }
    }
}
