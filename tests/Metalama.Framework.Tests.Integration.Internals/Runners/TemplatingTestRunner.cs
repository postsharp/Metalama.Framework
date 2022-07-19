// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.TestFramework;
using Metalama.TestFramework.Utilities;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;
#if NET5_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace Metalama.Framework.Tests.Integration.Runners
{
    /// <summary>
    /// Executes template integration tests by compiling and expanding a template method in the input source file.
    /// </summary>
    internal class TemplatingTestRunner : BaseTestRunner
    {
        private readonly IEnumerable<CSharpSyntaxVisitor> _testAnalyzers;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        public TemplatingTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger ) : this(
            serviceProvider,
            projectDirectory,
            references,
            Array.Empty<CSharpSyntaxVisitor>(),
            logger ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingTestRunner"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="projectDirectory"></param>
        /// <param name="metadataReferences"></param>
        /// <param name="implicitUsings"></param>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        /// <param name="logger"></param>
        public TemplatingTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            IEnumerable<CSharpSyntaxVisitor> testAnalyzers,
            ITestOutputHelper? logger )
            : base(
                serviceProvider,
                projectDirectory,
                references,
                logger )
        {
            this._testAnalyzers = testAnalyzers;
        }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <param name="testResult"></param>
        /// <param name="state"></param>
        /// <returns>The result of the test execution.</returns>
        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

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

            var serviceProvider = testResult.ProjectScopedServiceProvider;
            var assemblyLocator = serviceProvider.GetRequiredService<ReferenceAssemblyLocator>();

            // Create an empty compilation (just with references) for the compile-time project.
            var compileTimeCompilation = CSharpCompilation.Create(
                    "assemblyName",
                    Array.Empty<SyntaxTree>(),
                    assemblyLocator.StandardCompileTimeMetadataReferences,
                    (CSharpCompilationOptions) testResult.InputProject!.CompilationOptions! )
                .AddReferences( MetadataReference.CreateFromFile( typeof(TestTemplateAttribute).Assembly.Location ) );

            var templateCompiler = new TestTemplateCompiler( templateSemanticModel, testResult.PipelineDiagnostics, serviceProvider );

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
            var generatedDirectoryPath = Path.Combine( testInput.ProjectDirectory, "obj", testInput.ProjectProperties.TargetFramework, "generated" );
            var transformedTemplatePath = Path.Combine( generatedDirectoryPath, Path.ChangeExtension( testInput.TestName, ".cs" ) );
            var transformedTemplateText = await transformedTemplateSyntax!.SyntaxTree.GetTextAsync();
            Directory.CreateDirectory( Path.GetDirectoryName( transformedTemplatePath )! );

            var textWriter = new StreamWriter( transformedTemplatePath, false, Encoding.UTF8 );

            using ( textWriter.IgnoreAsyncDisposable() )
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
                (CSharpSyntaxNode) await oldTransformedTemplateSyntaxTree.GetRootAsync(),
                (CSharpParseOptions?) oldTransformedTemplateSyntaxTree.Options,
                transformedTemplatePath,
                Encoding.UTF8 );

            // Compile the template. This would eventually need to be done by Metalama itself and not this test program.
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
#if NET5_0_OR_GREATER
            var assemblyLoadContext = new AssemblyLoadContext( null, true );

            var assembly = assemblyLoadContext.LoadFromStream( buildTimeAssemblyStream, buildTimeDebugStream );
#else
            var assembly = Assembly.Load( buildTimeAssemblyStream.GetBuffer(), buildTimeDebugStream.GetBuffer() );
#endif

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
                var driver = new TemplateDriver( serviceProvider, compiledTemplateMethod );

                var compilationModel = CompilationModel.CreateInitialInstance(
                    new NullProject( serviceProvider ),
                    (CSharpCompilation) testResult.InputCompilation );

                var fakeTemplateClassMember = new TemplateClassMember(
                    "Template",
                    "Template",
                    null!,
                    TemplateInfo.None,
                    default,
                    ImmutableArray<TemplateClassMemberParameter>.Empty,
                    ImmutableArray<TemplateClassMemberParameter>.Empty,
                    ImmutableDictionary<MethodKind, TemplateClassMember>.Empty );

                var template = TemplateMemberFactory.Create( compilationModel.Factory.GetMethod( templateMethod ), fakeTemplateClassMember );

                var (expansionContext, targetMethod) = CreateTemplateExpansionContext(
                    serviceProvider,
                    assembly,
                    compilationModel,
                    template.ForIntroduction() );

                var expandSuccessful = driver.TryExpandDeclaration( expansionContext, Array.Empty<object>(), out var output );

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
#if NET5_0_OR_GREATER
            finally
            {
                assemblyLoadContext.Unload();
            }
#endif
        }

        private static (TemplateExpansionContext Context, MethodDeclarationSyntax TargetMethod) CreateTemplateExpansionContext(
            ServiceProvider serviceProvider,
            Assembly assembly,
            CompilationModel compilation,
            BoundTemplateMethod template )
        {
            var roslynCompilation = compilation.RoslynCompilation;

            var templateType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            var templateInstance = Activator.CreateInstance( templateType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetMetalamaType = compilation.Factory.GetTypeByReflectionName( targetType.FullName! );
            var targetMethod = targetMetalamaType.Methods.Single( m => string.Equals( m.Name, "Method", StringComparison.Ordinal ) );

            var diagnostics = new UserDiagnosticSink();

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
            var lexicalScope = LexicalScopeFactory.GetSourceLexicalScope( targetMethod );
            var syntaxGenerationContext = SyntaxGenerationContext.Create( serviceProvider, compilation.RoslynCompilation );

            var proceedExpression =
                new BuiltUserExpression(
                    GetProceedInvocation( targetMethod ),
                    targetMethod.ReturnType );

            var augmentedServiceProvider = serviceProvider.WithService( new AspectPipelineDescription( ExecutionScenario.CompileTime, true ) );

            var metaApi = MetaApi.ForMethod(
                targetMethod,
                new MetaApiProperties(
                    compilation,
                    diagnostics,
                    template.Template.Cast(),
                    ObjectReader.GetReader( new { TestKey = "TestValue" } ),
                    default,
                    syntaxGenerationContext,
                    null!,
                    augmentedServiceProvider,
                    MetaApiStaticity.Default ) );

            return (new TemplateExpansionContext(
                        templateInstance,
                        metaApi,
                        lexicalScope,
                        serviceProvider.GetRequiredService<SyntaxSerializationService>(),
                        syntaxGenerationContext,
                        template.Template,
                        proceedExpression,
                        default ),
                    roslynTargetMethod);

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
    }
}