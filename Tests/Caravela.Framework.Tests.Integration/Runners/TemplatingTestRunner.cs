// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    /// <summary>
    /// Executes template integration tests by compiling and expanding a template method in the input source file.
    /// </summary>
    internal class TemplatingTestRunner : BaseTestRunner
    {
        private static string GeneratedDirectoryPath => Path.Combine( Environment.CurrentDirectory, "generated" );

        private readonly IEnumerable<CSharpSyntaxVisitor> _testAnalyzers;
        private readonly SyntaxSerializationService _syntaxSerializationService = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        public TemplatingTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger ) : this(
            serviceProvider,
            projectDirectory,
            metadataReferences,
            Array.Empty<CSharpSyntaxVisitor>(),
            logger ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public TemplatingTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            IEnumerable<CSharpSyntaxVisitor> testAnalyzers,
            ITestOutputHelper? logger )
            : base(
                serviceProvider,
                projectDirectory,
                metadataReferences,
                logger )
        {
            this._testAnalyzers = testAnalyzers;
        }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <returns>The result of the test execution.</returns>
        public override TestResult RunTest( TestInput testInput )
        {
            var testResult = base.RunTest( testInput );

            if ( !testResult.Success )
            {
                return testResult;
            }

            var testSyntaxTree = testResult.SyntaxTrees.Single();
            var templateDocument = testSyntaxTree.InputDocument;
            var templateSyntaxRoot = templateDocument.GetSyntaxRootAsync().Result!;
            var templateSemanticModel = templateDocument.GetSemanticModelAsync().Result!;

            foreach ( var testAnalyzer in this._testAnalyzers )
            {
                testAnalyzer.Visit( templateSyntaxRoot );
            }

            var assemblyLocator = this.ServiceProvider.GetService<ReferenceAssemblyLocator>();

            // Create an empty compilation (just with references) for the compile-time project.
            var compileTimeCompilation = CSharpCompilation.Create(
                    "assemblyName",
                    Array.Empty<SyntaxTree>(),
                    assemblyLocator.StandardCompileTimeMetadataReferences,
                    (CSharpCompilationOptions) testResult.InputProject!.CompilationOptions! )
                .AddReferences( MetadataReference.CreateFromFile( typeof(TestTemplateAttribute).Assembly.Location ) );

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel, testResult, this.ServiceProvider );

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

            testSyntaxTree.AnnotatedSyntaxRoot = annotatedTemplateSyntax;

            // Write the transformed code to disk.
            var transformedTemplatePath = Path.Combine( GeneratedDirectoryPath, Path.ChangeExtension( testInput.TestName, ".cs" ) );
            var transformedTemplateText = transformedTemplateSyntax!.SyntaxTree.GetText();
            Directory.CreateDirectory( Path.GetDirectoryName( transformedTemplatePath ) );

            using ( var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 ) )
            {
                transformedTemplateText.Write( textWriter );
            }

            // Report the result to the test.
            testSyntaxTree.SetCompileTimeCode( transformedTemplateSyntax, transformedTemplatePath );

            if ( !templateCompilerSuccess )
            {
                testResult.SetFailed( "TestTemplateCompiler.TryCompile failed." );

                return testResult;
            }

            // Create a SyntaxTree that maps to the file we have just written.
            var oldTransformedTemplateSyntaxTree = transformedTemplateSyntax.SyntaxTree;

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
                testResult.Report( emitResult.Diagnostics );
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

                var templateMethod = testResult.InputCompilation!.Assembly.GetTypes().Single( t => t.Name == "Aspect" ).GetMembers( "Template" ).Single();

                Invariant.Assert( compiledTemplateMethod != null );
                var driver = new TemplateDriver( null!, templateMethod, compiledTemplateMethod );

                var compilationModel = CompilationModel.CreateInitialInstance( (CSharpCompilation) testResult.InputCompilation );
                var (expansionContext, targetMethod) = this.CreateTemplateExpansionContext( assembly, compilationModel, templateMethod );

                var expandSuccessful = driver.TryExpandDeclaration( expansionContext, testResult, out var output );

                testResult.Report( expansionContext.DiagnosticSink.ToImmutable().ReportedDiagnostics );

                if ( !expandSuccessful )
                {
                    testResult.SetFailed( "The compiled template failed." );

                    return testResult;
                }

                testSyntaxTree.SetRunTimeCode( targetMethod.WithBody( output! ) );
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

        private (TemplateExpansionContext Context, MethodDeclarationSyntax TargetMethod) CreateTemplateExpansionContext(
            Assembly assembly,
            CompilationModel compilation,
            ISymbol templateMethod )
        {
            var roslynCompilation = compilation.RoslynCompilation;

            var templateType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            var templateInstance = Activator.CreateInstance( templateType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetCaravelaType = compilation.Factory.GetTypeByReflectionName( targetType.FullName! )!;
            var targetMethod = targetCaravelaType.Methods.Single( m => m.Name == "Method" );

            var diagnostics = new UserDiagnosticSink( null, targetMethod );

            var roslynTargetType = roslynCompilation.Assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );

            var roslynTargetMethod = (MethodDeclarationSyntax) roslynTargetType.GetMembers()
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

            // ReSharper disable once SuspiciousTypeConversion.Global
            var lexicalScope = new TemplateLexicalScope( ((Declaration) targetMethod).LookupSymbols() );

            var proceedExpression =
                new DynamicExpression(
                    GetProceedInvocation( targetMethod ),
                    targetMethod.ReturnType,
                    false );

            var metaApi = MetaApi.ForMethod(
                targetMethod,
                new MetaApiProperties(
                    diagnostics,
                    templateMethod,
                    ImmutableDictionary.Create<string, object?>().Add( "TestKey", "TestValue" ),
                    default,
                    new AspectPipelineDescription( AspectExecutionScenario.CompileTime, true ),
                    proceedExpression ) );

            return (new TemplateExpansionContext(
                templateInstance,
                metaApi,
                compilation,
                lexicalScope,
                this._syntaxSerializationService,
                compilation.Factory ), roslynTargetMethod);

            static ExpressionSyntax GetProceedInvocation( Code.IMethod targetMethod )
            {
                return
                    SyntaxFactory.InvocationExpression(
                        targetMethod.IsStatic
                        ? SyntaxFactory.IdentifierName( targetMethod.Name )
                        : SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName( targetMethod.Name ) ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                targetMethod.Parameters.Select( x =>
                                    SyntaxFactory.Argument( SyntaxFactory.IdentifierName( x.Name ) ) ) ) ) );
            }
        }
    }
}