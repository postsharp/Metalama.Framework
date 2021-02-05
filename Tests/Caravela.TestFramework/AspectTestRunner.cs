using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    public class AspectTestRunner
    {
        public virtual async Task<TestResult> Run( string testSource )
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
            this.ReportDiagnostics( result, diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.TestErrorMessage = "Initial diagnostics failed.";
                return result;
            }

            try
            {
                var aspectPipeline = new AspectPipeline();
                var resultCompilation = aspectPipeline.Execute( new AspectTestPipelineContext( initialCompilation, result ) );

                var formattedOutput = Formatter.Format( resultCompilation.SyntaxTrees.Single().GetRoot(), project.Solution.Workspace );
                result.TemplateOutputSource = formattedOutput.GetText();

                return result;
            }
            catch ( Exception exception )
            {
                result.TestErrorMessage = exception.ToString();
                return result;
            }
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
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateHelper ).Assembly.Location ) )
                ;
            return project;
        }

        protected virtual IEnumerable<CSharpSyntaxVisitor> GetTestAnalyzers()
        {
            yield break;
        }

        protected virtual void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            result.Diagnostics.AddRange( diagnostics );
        }

        private class AspectTestPipelineContext : IAspectPipelineContext
        {
            private readonly TestResult _testResult;

            public AspectTestPipelineContext( Compilation compilation, TestResult testResult )
            {
                this.Compilation = compilation;
                this._testResult = testResult;
                this.ManifestResources = new List<ResourceDescription>();
            }

            public Compilation Compilation { get; }
            public ImmutableArray<object> Plugins => ImmutableArray<object>.Empty;
            public IList<ResourceDescription> ManifestResources { get; }

            public bool GetOptionsFlag( string flagName ) => false;

            public void ReportDiagnostic( Diagnostic diagnostic )
            {
                this._testResult.Diagnostics.Add( diagnostic );
            }
        }
    }
}

