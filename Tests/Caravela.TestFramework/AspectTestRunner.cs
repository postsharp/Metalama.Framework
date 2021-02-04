﻿using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    public class AspectTestRunner
    {
        public virtual async Task<TestResult> Run( string testSource )
        {
            TestResult result = new TestResult();
            testSource = CommonSnippets.CaravelaUsings + testSource;

            // Source.
            var project = this.CreateProject();
            var testDocument = project.AddDocument( "Test.cs", testSource );
            result.TemplateDocument = testDocument;

            var compilationForInitialDiagnostics = CSharpCompilation.Create(
                "assemblyName",
                new[] { await testDocument.GetSyntaxTreeAsync() },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions );
            var diagnostics = compilationForInitialDiagnostics.GetDiagnostics();
            this.ReportDiagnostics( result, diagnostics );

            if ( diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                result.TestErrorMessage = "Initial diagnostics failed.";
                return result;
            }

            var testSyntaxRoot = await testDocument.GetSyntaxRootAsync();
            var testSemanticModel = await testDocument.GetSemanticModelAsync();

            foreach ( var testAnalyzer in this.GetTestAnalyzers() )
            {
                testAnalyzer.Visit( testSyntaxRoot );
            }

            var templateCompiler = new TestTemplateCompiler( testSemanticModel );
            bool success = templateCompiler.TryCompile( testSyntaxRoot, out var annotatedSyntaxRoot, out var transformedSyntaxRoot );
            result.AnnotatedSyntaxRoot = annotatedSyntaxRoot;
            result.TransformedSyntaxRoot = transformedSyntaxRoot;

            this.ReportDiagnostics( result, templateCompiler.Diagnostics );

            if ( !success )
            {
                result.TestErrorMessage = "Template compiler failed.";
                return result;
            }

            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.
            var finalCompilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { transformedSyntaxRoot.SyntaxTree.WithFilePath( string.Empty ) },
                project.MetadataReferences,
                (CSharpCompilationOptions) project.CompilationOptions );

            var buildTimeAssemblyStream = new MemoryStream();
            var buildTimeDebugStream = new MemoryStream();

            var emitResult = finalCompilation.Emit(
                buildTimeAssemblyStream, buildTimeDebugStream,
                options: new EmitOptions(
                    defaultSourceFileEncoding: Encoding.UTF8,
                    fallbackSourceFileEncoding: Encoding.UTF8 ) );

            if ( !emitResult.Success )
            {
                this.ReportDiagnostics( result, emitResult.Diagnostics );
                result.TestErrorMessage = "Final compilation failed.";
                return result;
            }

            buildTimeAssemblyStream.Seek( 0, SeekOrigin.Begin );
            buildTimeDebugStream?.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var aspectType = assembly.GetTypes().First( t => t.Name.Equals( "Aspect" ) );
                var targetType = assembly.GetTypes().First( t => t.Name.Equals( "TargetCode" ) );
                var aspectInstance = Activator.CreateInstance( aspectType );
                var templateMethod = aspectType.GetMethod( "Template_Template", BindingFlags.Instance | BindingFlags.Public );

                Debug.Assert( templateMethod != null );

                var targetTypeSymbol = finalCompilation.Assembly.GetTypes().First( t => t.Name.Equals( "TargetCode" ) );
                var targetMethodSymbol = (IMethodSymbol) targetTypeSymbol.GetMembers().SingleOrDefault( m => m.Name == "Method" );

                var driver = new TemplateDriver( templateMethod );

                var caravelaCompilation = new SourceCompilation( finalCompilation );
                var targetCaravelaType = caravelaCompilation.GetTypeByReflectionName( targetType.FullName );
                var targetCaravelaMethod = targetCaravelaType.Methods.GetValue().SingleOrDefault( m => m.Name == "Method" );

                var output = driver.ExpandDeclaration( aspectInstance, targetCaravelaMethod, caravelaCompilation );
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
    }
}

