// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
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
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using RefKind = Caravela.Framework.Code.RefKind;

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
        /// <param name="testResult1"></param>
        /// <param name="state"></param>
        /// <returns>The result of the test execution.</returns>
        private protected override async Task RunAsync( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput,testResult, state );

            if ( !testResult.Success )
            {
                return;
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

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel, testResult.PipelineDiagnostics, this.ServiceProvider );

            var templateCompilerSuccess = templateCompiler.TryCompile(
                compileTimeCompilation,
                templateSyntaxRoot,
                out var annotatedTemplateSyntax,
                out var transformedTemplateSyntax );

            // Annotation shouldn't do any code transformations.
            // Otherwise, highlighted spans don't match the actual code.
            if ( templateCompilerSuccess && templateSyntaxRoot != null && annotatedTemplateSyntax != null )
            {
                Assert.Equal( templateSyntaxRoot.ToString(), annotatedTemplateSyntax.ToString() );
            }

            testSyntaxTree.AnnotatedSyntaxRoot = annotatedTemplateSyntax;

            // Write the transformed code to disk.
            var transformedTemplatePath = Path.Combine( GeneratedDirectoryPath, Path.ChangeExtension( testInput.TestName, ".cs" ) );
            var transformedTemplateText = await transformedTemplateSyntax!.SyntaxTree.GetTextAsync();
            Directory.CreateDirectory( Path.GetDirectoryName( transformedTemplatePath )! );

            await using ( var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 ) )
            {
                transformedTemplateText.Write( textWriter );
            }

            // Report the result to the test.
            testSyntaxTree.SetCompileTimeCode( transformedTemplateSyntax, transformedTemplatePath );

            if ( !templateCompilerSuccess )
            {
                testResult.SetFailed( "TestTemplateCompiler.TryCompile failed." );

                return;
            }

            testResult.HasOutputCode = true;

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

            // SyntaxTreeStructureVerifier.Verify( compileTimeCompilation );

            var emitResult = compileTimeCompilation.Emit(
                buildTimeAssemblyStream,
                buildTimeDebugStream,
                options: new EmitOptions(
                    defaultSourceFileEncoding: Encoding.UTF8,
                    fallbackSourceFileEncoding: Encoding.UTF8 ) );

            if ( !emitResult.Success )
            {
                testResult.PipelineDiagnostics.Report( emitResult.Diagnostics );
                testResult.SetFailed( "The final template compilation failed." );

                return;
            }

            if ( !this.VerifyBinaryStream( testInput, testResult, buildTimeAssemblyStream ) )
            {
                return;
            }

            buildTimeAssemblyStream.Seek( 0, SeekOrigin.Begin );
            buildTimeDebugStream.Seek( 0, SeekOrigin.Begin );
            var assemblyLoadContext = new AssemblyLoadContext( null, true );
            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );

            try
            {
                var templateMethod = testResult.InputCompilation!.Assembly.GetTypes()
                    .Single( t => string.Equals( t.Name, "Aspect", StringComparison.Ordinal ) )
                    .GetMembers( "Template" )
                    .OfType<IMethodSymbol>()
                    .Single();

                var compiledAspectType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );

                var compiledTemplateMethodName = TemplateNameHelper.GetCompiledTemplateName( templateMethod );
                var compiledTemplateMethod = compiledAspectType.GetMethod( compiledTemplateMethodName, BindingFlags.Instance | BindingFlags.Public );

                Invariant.Assert( compiledTemplateMethod != null );
                var driver = new TemplateDriver( this.ServiceProvider, null!, templateMethod, compiledTemplateMethod );

                var compilationModel = CompilationModel.CreateInitialInstance( (CSharpCompilation) testResult.InputCompilation );
                var template = Template.Create<IMemberOrNamedType>( compilationModel.Factory.GetMethod( templateMethod ), TemplateInfo.None );

                var (expansionContext, targetMethod) = this.CreateTemplateExpansionContext( this.ServiceProvider, assembly, compilationModel, template );

                var expandSuccessful = driver.TryExpandDeclaration( expansionContext, testResult.PipelineDiagnostics, out var output );

                testResult.PipelineDiagnostics.Report( expansionContext.DiagnosticSink.ToImmutable().ReportedDiagnostics );

                if ( !expandSuccessful )
                {
                    testResult.SetFailed( "The compiled template failed." );

                    return;
                }

                await testSyntaxTree.SetRunTimeCodeAsync( targetMethod.WithBody( output! ) );
            }
            catch ( Exception e )
            {
                testResult.SetFailed( "Exception during template expansion: " + e.Message, e );
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            return;
        }

        private (TemplateExpansionContext Context, MethodDeclarationSyntax TargetMethod) CreateTemplateExpansionContext(
            IServiceProvider serviceProvider,
            Assembly assembly,
            CompilationModel compilation,
            Template<IMemberOrNamedType> template )
        {
            var roslynCompilation = compilation.RoslynCompilation;

            var templateType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            var templateInstance = Activator.CreateInstance( templateType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetCaravelaType = compilation.Factory.GetTypeByReflectionName( targetType.FullName! );
            var targetMethod = targetCaravelaType.Methods.Single( m => string.Equals( m.Name, "Method", StringComparison.Ordinal ) );

            var diagnostics = new UserDiagnosticSink( targetMethod );

            var roslynTargetType = roslynCompilation.Assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );

            var roslynTargetMethod = (MethodDeclarationSyntax) roslynTargetType.GetMembers()
                .Single( m => string.Equals( m.Name, "Method", StringComparison.Ordinal ) )
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
                    targetMethod.ReturnType );

            var additionalServices = new ServiceProvider();
            additionalServices.AddService( new AspectPipelineDescription( AspectExecutionScenario.CompileTime, true ) );
            var augmentedServiceProvider = new AggregateServiceProvider( serviceProvider, additionalServices );

            var metaApi = MetaApi.ForMethod(
                targetMethod,
                new MetaApiProperties(
                    diagnostics,
                    template,
                    ImmutableDictionary.Create<string, object?>().Add( "TestKey", "TestValue" ),
                    default,
                    augmentedServiceProvider ) );

            return (new TemplateExpansionContext(
                        templateInstance,
                        metaApi,
                        compilation,
                        lexicalScope,
                        this._syntaxSerializationService,
                        compilation.Factory,
                        default,
                        proceedExpression ), roslynTargetMethod);

            static ExpressionSyntax GetProceedInvocation( IMethod targetMethod )
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
                                targetMethod.Parameters.Select(
                                    p =>
                                        SyntaxFactory.Argument(
                                            null,
                                            p.RefKind switch
                                            {
                                                RefKind.None => default,
                                                RefKind.In => default,
                                                RefKind.Out => SyntaxFactory.Token( SyntaxKind.OutKeyword ),
                                                RefKind.Ref => SyntaxFactory.Token( SyntaxKind.RefKeyword ),
                                                _ => throw new AssertionFailedException()
                                            },
                                            SyntaxFactory.IdentifierName( p.Name ) ) ) ) ) );
            }
        }

        private class AggregateServiceProvider : IServiceProvider
        {
            private readonly IReadOnlyList<IServiceProvider> _children;

            public AggregateServiceProvider( params IServiceProvider[] children )
            {
                this._children = children;
            }

            public object? GetService( Type serviceType ) => this._children.Select( c => c.GetService( serviceType ) ).FirstOrDefault( s => s != null );
        }
    }
}