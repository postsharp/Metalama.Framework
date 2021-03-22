using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Formatting;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Templating
{
    /// <summary>
    /// Executes template integration tests by compiling and expanding a template method in the input source file.
    /// </summary>
    internal class TemplateTestRunner : TemplatingTestRunnerBase
    {
        private static string GeneratedDirectoryPath => Path.Combine( Environment.CurrentDirectory, "generated" );

        private readonly IEnumerable<CSharpSyntaxVisitor> _testAnalyzers;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTestRunner"/> class.
        /// </summary>
        public TemplateTestRunner() : this( Array.Empty<CSharpSyntaxVisitor>() )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTestRunner"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public TemplateTestRunner( IEnumerable<CSharpSyntaxVisitor> testAnalyzers )
        {
            this._testAnalyzers = testAnalyzers;
        }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <returns>The result of the test execution.</returns>
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

            foreach ( var testAnalyzer in this._testAnalyzers )
            {
                testAnalyzer.Visit( templateSyntaxRoot );
            }

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel );
            var templateCompilerSuccess = templateCompiler.TryCompile( templateSyntaxRoot, out var annotatedTemplateSyntax, out var transformedTemplateSyntax );

            this.ReportDiagnostics( result, templateCompiler.Diagnostics );

            if ( !templateCompilerSuccess )
            {
                result.ErrorMessage = "Template compiler failed.";
                return result;
            }

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax.ToString() );

            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;
            result.TransformedTemplateSyntax = transformedTemplateSyntax;

            // Write the transformed code to disk.
            var transformedTemplateText = result.TransformedTemplateSyntax!.SyntaxTree.GetText();
            var transformedTemplatePath = Path.Combine( GeneratedDirectoryPath, Path.ChangeExtension( testInput.TestName, ".cs" ) );
            Directory.CreateDirectory( Path.GetDirectoryName( transformedTemplatePath ) );

            using ( var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 ) )
            {
                transformedTemplateText.Write( textWriter );
            }

            // Create a SyntaxTree that maps to the file we have just written.
            var oldTransformedTemplateSyntaxTree = result.TransformedTemplateSyntax.SyntaxTree;
            var newTransformedTemplateSyntaxTree = CSharpSyntaxTree.Create(
                (CSharpSyntaxNode) oldTransformedTemplateSyntaxTree.GetRoot(),
                (CSharpParseOptions?) oldTransformedTemplateSyntaxTree.Options,
                transformedTemplatePath,
                Encoding.UTF8 );

            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.
            var finalCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { newTransformedTemplateSyntaxTree },
                result.Project.MetadataReferences,
                (CSharpCompilationOptions) result.Project.CompilationOptions! );

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
            buildTimeDebugStream.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var aspectType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
                var templateMethod = aspectType.GetMethod( "Template_Template", BindingFlags.Instance | BindingFlags.Public );

                Invariant.Assert( templateMethod != null );
                var driver = new TemplateDriver( templateMethod );

                var caravelaCompilation = CompilationModel.CreateInitialInstance( (CSharpCompilation) result.InitialCompilation );
                var expansionContext = new TestTemplateExpansionContext( assembly, caravelaCompilation );

                var output = driver.ExpandDeclaration( expansionContext );
                var formattedOutput = Formatter.Format( output, result.Project.Solution.Workspace );

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
    }
}
