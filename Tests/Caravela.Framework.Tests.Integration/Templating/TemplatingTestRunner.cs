// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Templating
{
    /// <summary>
    /// Executes template integration tests by compiling and expanding a template method in the input source file.
    /// </summary>
    internal class TemplatingTestRunner : TestRunnerBase
    {
        private static string GeneratedDirectoryPath => Path.Combine( Environment.CurrentDirectory, "generated" );

        private readonly IEnumerable<CSharpSyntaxVisitor> _testAnalyzers;
        private readonly SyntaxSerializationService _syntaxSerializationService = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        public TemplatingTestRunner( string? projectDirectory = null ) : this( projectDirectory, Array.Empty<CSharpSyntaxVisitor>() ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public TemplatingTestRunner( string? projectDirectory, IEnumerable<CSharpSyntaxVisitor> testAnalyzers ) : base( projectDirectory )
        {
            this._testAnalyzers = testAnalyzers;
        }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <returns>The result of the test execution.</returns>
        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var testResult = await base.RunTestAsync( testInput );

            if ( !testResult.Success )
            {
                return testResult;
            }

            var templateSyntaxRoot = (await testResult.TemplateDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await testResult.TemplateDocument.GetSemanticModelAsync())!;

            foreach ( var testAnalyzer in this._testAnalyzers )
            {
                testAnalyzer.Visit( templateSyntaxRoot );
            }

            // Create an empty compilation (just with references) for the compile-time project.
            var compileTimeCompilation = CSharpCompilation.Create(
                "assemblyName",
                Array.Empty<SyntaxTree>(),
                testResult.Project.MetadataReferences,
                (CSharpCompilationOptions) testResult.Project.CompilationOptions! );

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel, testResult );

            var templateCompilerSuccess = templateCompiler.TryCompile(
                compileTimeCompilation,
                templateSyntaxRoot,
                out var annotatedTemplateSyntax,
                out var transformedTemplateSyntax );

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            if ( templateSyntaxRoot != null && annotatedTemplateSyntax != null )
            {
                Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax.ToString() );
            }

            testResult.AnnotatedTemplateSyntax = annotatedTemplateSyntax;
            testResult.TransformedTemplateSyntax = transformedTemplateSyntax;

            if ( !templateCompilerSuccess )
            {
                testResult.SetFailed( "TestTemplateCompiler.TryCompile failed." );

                return testResult;
            }

            // Write the transformed code to disk.
            var transformedTemplateText = testResult.TransformedTemplateSyntax!.SyntaxTree.GetText();
            var transformedTemplatePath = Path.Combine( GeneratedDirectoryPath, Path.ChangeExtension( testInput.TestName, ".cs" ) );
            testResult.TransformedTemplatePath = transformedTemplatePath;
            Directory.CreateDirectory( Path.GetDirectoryName( transformedTemplatePath ) );

            await using ( var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 ) )
            {
                transformedTemplateText.Write( textWriter );
            }

            // Create a SyntaxTree that maps to the file we have just written.
            var oldTransformedTemplateSyntaxTree = testResult.TransformedTemplateSyntax.SyntaxTree;

            var newTransformedTemplateSyntaxTree = CSharpSyntaxTree.Create(
                (CSharpSyntaxNode) oldTransformedTemplateSyntaxTree.GetRoot(),
                (CSharpParseOptions?) oldTransformedTemplateSyntaxTree.Options,
                transformedTemplatePath,
                Encoding.UTF8 );

            // Compile the template. This would eventually need to be done by Caravela itself and not this test program.
            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( newTransformedTemplateSyntaxTree );

            var buildTimeAssemblyStream = new MemoryStream();
            var buildTimeDebugStream = new MemoryStream();

            var emitResult = compileTimeCompilation.Emit(
                buildTimeAssemblyStream,
                buildTimeDebugStream,
                options: new EmitOptions(
                    defaultSourceFileEncoding: Encoding.UTF8,
                    fallbackSourceFileEncoding: Encoding.UTF8 ) );

            if ( !emitResult.Success )
            {
                testResult.ReportDiagnostics( emitResult.Diagnostics );
                testResult.SetFailed( "The final template compilation failed." );

                return testResult;
            }

            buildTimeAssemblyStream.Seek( 0, SeekOrigin.Begin );
            buildTimeDebugStream.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var compiledAspectType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
                var compiledTemplateMethod = compiledAspectType.GetMethod( "Template_Template", BindingFlags.Instance | BindingFlags.Public );

                var templateMethod = testResult.InitialCompilation.Assembly.GetTypes().Single( t => t.Name == "Aspect" ).GetMembers( "Template" ).Single();

                Invariant.Assert( compiledTemplateMethod != null );
                var driver = new TemplateDriver( templateMethod, compiledTemplateMethod );

                var compilationModel = CompilationModel.CreateInitialInstance( (CSharpCompilation) testResult.InitialCompilation );
                var expansionContext = this.CreateTemplateExpansionContext( assembly, compilationModel );

                if ( !driver.TryExpandDeclaration( expansionContext, testResult, out var output ) )
                {
                    testResult.SetFailed( "The compiled template threw an exception." );

                    return testResult;
                }

                testResult.SetTransformedTarget( output );
            }
            catch ( Exception e )
            {
                testResult.SetFailed( "Exception during template expansion: " + e.Message, e );
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            return testResult;
        }

        private TemplateExpansionContext CreateTemplateExpansionContext( Assembly assembly, CompilationModel compilation )
        {
            var roslynCompilation = compilation.RoslynCompilation;

            var templateType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            var templateInstance = Activator.CreateInstance( templateType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetCaravelaType = compilation.Factory.GetTypeByReflectionName( targetType.FullName! )!;
            var targetMethod = targetCaravelaType.Methods.Single( m => m.Name == "Method" );

            var diagnostics = new DiagnosticSink( targetMethod );

            var roslynTargetType = roslynCompilation.Assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );

            var roslynTargetMethod = (BaseMethodDeclarationSyntax) roslynTargetType.GetMembers()
                .Single( m => m.Name == "Method" )
                .DeclaringSyntaxReferences
                .Select( r => (CSharpSyntaxNode) r.GetSyntax() )
                .Single();

            var semanticModel = compilation.RoslynCompilation.GetSemanticModel( compilation.RoslynCompilation.SyntaxTrees.First() );
            var roslynTargetMethodSymbol = semanticModel.GetDeclaredSymbol( roslynTargetMethod );

            if ( roslynTargetMethodSymbol == null )
            {
                throw new InvalidOperationException( "The symbol of the target method was not found." );
            }

            var lexicalScope = new TemplateLexicalScope( ((CodeElement) targetMethod).LookupSymbols() );

            var syntaxFactory = ReflectionMapper.GetInstance( compilation.RoslynCompilation );

            return new TemplateExpansionContext(
                templateInstance,
                targetMethod,
                compilation,
                new LinkerOverrideProceedImpl(
                    default,
                    targetMethod,
                    LinkerAnnotationOrder.Default,
                    syntaxFactory ),
                lexicalScope,
                diagnostics,
                this._syntaxSerializationService,
                syntaxFactory );
        }
    }
}