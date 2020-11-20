using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Caravela.TestFramework.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Caravela.TestFramework.Templating
{
    public class TestRunner
    {
        public virtual async Task<TestResult> Run( string testInput )
        {
            TestResult result = new TestResult();

            // Source.
            var project = this.CreateProject();
            var document1 = project.AddDocument( "TestInput.cs", testInput );
            result.InputDocument = document1;

            var compilationForInitialDiagnostics = CSharpCompilation.Create(
                "assemblyName",
                new[] { await document1.GetSyntaxTreeAsync() },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions );
            var diagnostics = compilationForInitialDiagnostics.GetDiagnostics();
            this.ReportDiagnostics( result, diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.TestErrorMessage = "Initial diagnostics failed.";
                return result;
            }

            var syntaxRoot1 = await document1.GetSyntaxRootAsync();
            var semanticModel1 = await document1.GetSemanticModelAsync();

            var templateCompiler = new TestTemplateCompiler( semanticModel1 );
            bool success = templateCompiler.TryCompile( syntaxRoot1, out var annotatedSyntaxRoot, out var transformedSyntaxRoot );
            result.AnnotatedSyntaxRoot = annotatedSyntaxRoot;
            result.TransformedSyntaxRoot = transformedSyntaxRoot;

            this.ReportDiagnostics( result, templateCompiler.Diagnostics );

            if (!success)
            {
                result.TestErrorMessage = "Template compiler failed.";
                return result;
            }

            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.
            var finalCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { transformedSyntaxRoot.SyntaxTree },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions );

            var buildTimeAssemblyStream = new MemoryStream();
            var buildTimeDebugStream = new MemoryStream();
            var emitResult = finalCompilation.Emit( buildTimeAssemblyStream, buildTimeDebugStream );
            if ( !emitResult.Success )
            {
                this.ReportDiagnostics( result, emitResult.Diagnostics );
                result.TestErrorMessage = "Final compilation failed.";
                return result;
            }
            
            buildTimeAssemblyStream.Seek( 0, SeekOrigin.Begin );
            buildTimeDebugStream.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var aspectType = assembly.GetType( "Aspect" );
                var aspectInstance = Activator.CreateInstance( aspectType );
                var templateMethod = aspectType.GetMethod( "Template_Template", BindingFlags.Instance | BindingFlags.Public );

                //var targetType = compilationForInitialDiagnostics.Assembly.GetTypeByMetadataName( "TargetCode" );
                //var targetMethod = (IMethodSymbol) targetType.GetMembers().SingleOrDefault( m => m.Name == "Method" );

                var driver = new TemplateDriver( templateMethod );

                var caravelaCompilation = new SourceCompilation( compilationForInitialDiagnostics );
                var targetType = caravelaCompilation.GetTypeByReflectionName( "TargetCode" );
                var targetMethod = targetType.Methods.GetValue().SingleOrDefault( m => m.Name == "Method" );

                //AdviceContext.Current = new AdviceContextImpl( targetMethod );

                var output = driver.ExpandDeclaration( aspectInstance, targetMethod, caravelaCompilation );
                var formattedOutput = Formatter.Format( output, project.Solution.Workspace );

                result.TemplateOutputSource = formattedOutput.GetText();
            }
            catch ( Exception exception )
            {
                result.TestErrorMessage = exception.ToString();
                return result;
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            return result;
        }

        protected virtual Project CreateProject()
        {
            var netStandardDirectory =
                Environment.ExpandEnvironmentVariables(
                    @"%USERPROFILE%\.nuget\packages\netstandard.library\2.0.0\build\netstandard2.0\ref" );

            var guid = Guid.NewGuid();
            var workspace1 = new AdhocWorkspace();
            var solution = workspace1.CurrentSolution;
            var project = solution.AddProject( guid.ToString(), guid.ToString(), LanguageNames.CSharp )
                .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddMetadataReferences( Directory.GetFiles( netStandardDirectory, "*.dll" )
                    .Where( f => !Path.GetFileNameWithoutExtension( f ).EndsWith( "Native", StringComparison.OrdinalIgnoreCase ) )
                    .Select( f => MetadataReference.CreateFromFile( f ) ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( AdviceContext ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( CompileTimeAttribute ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( SyntaxNode ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( SyntaxFactory ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( TemplateHelper ).Assembly.Location ) )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( Microsoft.CSharp.RuntimeBinder.Binder ).Assembly.Location ) )
                ;
            return project;
        }

        protected virtual void ReportDiagnostics( TestResult result, IReadOnlyList<Diagnostic> diagnostics )
        {
            result.Diagnostics.AddRange( diagnostics );
        }
    }
}
