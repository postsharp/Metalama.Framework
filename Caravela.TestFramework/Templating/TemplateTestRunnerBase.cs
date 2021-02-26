using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Caravela.TestFramework.Templating
{
    internal abstract class TemplateTestRunnerBase
    {
        private readonly IEnumerable<CSharpSyntaxVisitor> _testAnalyzers;

        protected static string GeneratedDirectoryPath => Path.Combine( Environment.CurrentDirectory, "generated" );

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTestRunnerBase"/> class.
        /// </summary>
        public TemplateTestRunnerBase() : this( Array.Empty<CSharpSyntaxVisitor>() )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTestRunnerBase"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public TemplateTestRunnerBase( IEnumerable<CSharpSyntaxVisitor> testAnalyzers )
        {
            this._testAnalyzers = testAnalyzers;
        }

        public virtual async Task<TestResult> RunAsync( TestInput testInput )
        {
            var testSource = CommonSnippets.CaravelaUsings + testInput.TestSource;

            //TODO: create test
            //var tree = CSharpSyntaxTree.ParseText( testSource );
            //TriviaAdder triviaAdder = new();
            //var templateRootWithTrivias = triviaAdder.Visit( tree.GetRoot() );

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

            var templateSyntaxRoot = (await testDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await testDocument.GetSemanticModelAsync())!;

            foreach ( var testAnalyzer in this._testAnalyzers )
            {
                testAnalyzer.Visit( templateSyntaxRoot );
            }

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel );
            var templateCompilerSuccess = templateCompiler.TryCompile( templateSyntaxRoot, out var annotatedTemplateSyntax, out var transformedTemplateSyntax );

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax.ToString() );

            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;
            result.TransformedTemplateSyntax = transformedTemplateSyntax;

            this.ReportDiagnostics( result, templateCompiler.Diagnostics );

            if ( !templateCompilerSuccess )
            {
                result.ErrorMessage = "Template compiler failed.";
                return result;
            }

            result.Success = true;

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
                    .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                    .AddMetadataReferences( referenceAssemblies.Select( f => MetadataReference.CreateFromFile( f ) ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( CompileTimeAttribute ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateSyntaxFactory ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TestTemplateAttribute ).Assembly.Location ) )
                    .AddMetadataReference( MetadataReference.CreateFromFile( typeof( IReactiveCollection<> ).Assembly.Location ) )
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
            result.Diagnostics.AddRange( diagnostics );
        }
    }
}
