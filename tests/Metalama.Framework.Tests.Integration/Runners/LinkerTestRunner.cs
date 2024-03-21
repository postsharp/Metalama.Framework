// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.Integration.Runners.Linker;
using Metalama.Testing.AspectTesting;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal sealed class LinkerTestRunner : BaseTestRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkerTestRunner"/> class.
        /// </summary>
        public LinkerTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            : base(
                serviceProvider,
                projectDirectory,
                references,
                logger ) { }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <param name="testResult"></param>
        /// <param name="testContext"></param>
        /// <param name="state"></param>
        /// <returns>The result of the test execution.</returns>
        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            TestContext testContext,
            Dictionary<string, object?> state )
        {
            // There is a chicken-or-egg in the design of the test because the project-scoped service provider is needed before the compilation
            // is created. We break the cycle by providing the service provider with the default set of references, which should work for 
            // the linker tests because they are not cross-assembly.
            var serviceProvider = (ProjectServiceProvider) testContext.ServiceProvider.Global.Underlying
                .WithProjectScopedServices( new DefaultProjectOptions(), TestCompilationFactory.GetMetadataReferences() )
                .WithService( SyntaxGenerationOptions.Proof );

            serviceProvider = serviceProvider.WithCompileTimeProjectServices( CompileTimeProjectRepository.CreateTestInstance() );

            var preliminaryCompilation = TestCompilationFactory.CreateEmptyCSharpCompilation(
                testInput.TestName,
                TestCompilationFactory.GetMetadataReferences() );

            var preliminaryCompilationContext = CompilationContextFactory.GetInstance( preliminaryCompilation );

            var builder = new LinkerTestInputBuilder( serviceProvider, preliminaryCompilationContext );

            state["builder"] = builder;

            await base.RunAsync( testInput, testResult, testContext, state );

            if ( !testResult.Success )
            {
                return;
            }

            // Create the linker input.
            var linkerInput = builder.ToAspectLinkerInput( PartialCompilation.CreateComplete( testResult.InputCompilation.AssertNotNull() ) );

            var linker = new AspectLinker( serviceProvider, linkerInput );

            var result = await linker.ExecuteAsync( CancellationToken.None );

            var linkedCompilation = result.Compilation;

            var cleanCompilation =
                LinkerTestInputBuilder.GetCleanCompilation( linkedCompilation )
                    .Compilation.AssertNotNull();

            testResult.OutputCompilation = cleanCompilation;
            testResult.HasOutputCode = true;

            await testResult.SetOutputCompilationAsync( cleanCompilation );

            // Attempt to Emit the result.
            var emitResult = cleanCompilation.Emit( Stream.Null );

            testResult.PipelineDiagnostics.Report( emitResult.Diagnostics );

            if ( !emitResult.Success )
            {
                testResult.SetFailed( "Final Compilation.Emit failed." );
            }

            if ( !SyntaxTreeStructureVerifier.Verify( cleanCompilation, out var diagnostics ) )
            {
                testResult.SetFailed( "Syntax tree verification failed." );
                testResult.OutputCompilationDiagnostics.Report( diagnostics );
            }
        }

        protected override void ExecuteAssertions( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            var assertionWalker = new LinkerInlineAssertionWalker();

            foreach ( var syntaxTree in testResult.SyntaxTrees )
            {
                assertionWalker.Visit( syntaxTree.OutputRunTimeSyntaxRoot );
            }

            base.ExecuteAssertions( testInput, testResult, state );
        }

        private protected override SyntaxNode PreprocessSyntaxRoot( SyntaxNode syntaxRoot, Dictionary<string, object?> state )
        {
            var builder = (LinkerTestInputBuilder) state["builder"]!;

            return builder.ProcessSyntaxRoot( syntaxRoot );
        }
    }
}