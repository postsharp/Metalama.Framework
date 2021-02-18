using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Project;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Formatting;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using Microsoft.CodeAnalysis.Text;
using Caravela.Framework.Impl;

namespace Caravela.TestFramework.Templating
{
    public class TestRunner
    {
        public virtual async Task<TestResult> Run( TestInput testInput )
        {
            var testSource = CommonSnippets.CaravelaUsings + testInput.TemplateSource;

            // Source.
            var project = this.CreateProject();
            var testDocument = project.AddDocument( "Test.cs", SourceText.From( testSource, encoding: Encoding.UTF8 ) );

            var result = new TestResult( testDocument );

            var compilationForInitialDiagnostics = CSharpCompilation.Create(
                "assemblyName",
                new[] { (await testDocument.GetSyntaxTreeAsync())! },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions! );
            var diagnostics = compilationForInitialDiagnostics.GetDiagnostics();
            this.ReportDiagnostics( result, diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.ErrorMessage = "Initial diagnostics failed.";
                return result;
            }

            var templateSyntaxRoot = (await testDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await testDocument.GetSemanticModelAsync())!;

            foreach ( var testAnalyzer in this.GetTestAnalyzers() )
            {
                // TODO: should visit only template method.
                testAnalyzer.Visit( templateSyntaxRoot );
            }

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel );
            var templaceCompilerSuccess = templateCompiler.TryCompile( templateSyntaxRoot, out var annotatedTemplateSyntax, out var transformedTemplateSyntax );
            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;
            result.TransformedTemplateSyntax = transformedTemplateSyntax;

            this.ReportDiagnostics( result, templateCompiler.Diagnostics );

            if ( !templaceCompilerSuccess )
            {
                result.ErrorMessage = "Template compiler failed.";
                return result;
            }

            // Write the transformed code to disk.
            var transformedTemplateText = transformedTemplateSyntax.SyntaxTree.GetText();
            var transformedTemplatePath = Path.Combine( Environment.CurrentDirectory, "generated", Path.ChangeExtension( testInput.TestName, ".cs" ) );
            var transformedTemplateDirectory = Path.GetDirectoryName( transformedTemplatePath );
            if ( !Directory.Exists(transformedTemplateDirectory))
            {
                Directory.CreateDirectory( transformedTemplateDirectory );
            }

            using ( var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 ) )
            {
                transformedTemplateText.Write( textWriter );
            }

            // Create a SyntaxTree that maps to the file we have just written.
            var oldTransformedTemplateSyntaxTree = transformedTemplateSyntax.SyntaxTree;
            var newTransformedTemplateSyntaxTree = CSharpSyntaxTree.Create( 
                (CSharpSyntaxNode) oldTransformedTemplateSyntaxTree.GetRoot(), 
                (CSharpParseOptions?) oldTransformedTemplateSyntaxTree.Options, 
                transformedTemplatePath, 
                Encoding.UTF8 );

            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.
            var finalCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { newTransformedTemplateSyntaxTree },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions! );

            var buildTimeAssemblyStream = new MemoryStream();
            var buildTimeDebugStream = new MemoryStream();

            var emitResult = finalCompilation.Emit(
                buildTimeAssemblyStream,
                buildTimeDebugStream,
                options: new EmitOptions(
                    defaultSourceFileEncoding: Encoding.UTF8,
                    fallbackSourceFileEncoding: Encoding.UTF8 ) );

            if ( !emitResult.Success )
            {
                this.ReportDiagnostics( result, emitResult.Diagnostics );
                
                result.ErrorMessage = "Final compilation failed.";
                return result;
            }

            buildTimeAssemblyStream.Seek( 0, SeekOrigin.Begin );
            buildTimeDebugStream?.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var aspectType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
                var templateMethod = aspectType.GetMethod( "Template_Template", BindingFlags.Instance | BindingFlags.Public );

                Debug.Assert( templateMethod != null, "Cannot find the template method." );
                var driver = new TemplateDriver( templateMethod );

                var caravelaCompilation = new CompilationModel( compilationForInitialDiagnostics );
                var expansionContext = new TestTemplateExpansionContext( assembly, caravelaCompilation );

                var output = driver.ExpandDeclaration( expansionContext );
                var formattedOutput = Formatter.Format( output, project.Solution.Workspace );

                result.TransformedTargetSource = formattedOutput.GetText();
            }
            catch ( Exception exception )
            {
                result.ErrorMessage = exception.ToString();
                return result;
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            result.Success = true;

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
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TestTemplateAttribute ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( IReactiveCollection<> ).Assembly.Location ) )
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
    }
}
