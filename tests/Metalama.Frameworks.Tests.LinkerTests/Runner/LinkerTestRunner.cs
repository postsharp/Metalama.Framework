﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Testing.AspectTesting;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.LinkerTests.Runner
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
            ITestOutputHelper? logger,
            ILicenseKeyProvider? licenseKeyProvider )
            : base(
                serviceProvider,
                projectDirectory,
                references,
                logger,
                licenseKeyProvider ) { }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <param name="testResult"></param>
        /// <param name="testContext"></param>
        /// <returns>The result of the test execution.</returns>
        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            TestContext testContext )
        {
            // There is a chicken-or-egg in the design of the test because the project-scoped service provider is needed before the compilation
            // is created. We break the cycle by providing the service provider with the default set of references, which should work for 
            // the linker tests because they are not cross-assembly.
            var serviceProvider = (ProjectServiceProvider) testContext.ServiceProvider.Global.Underlying
                .WithProjectScopedServices( new DefaultProjectOptions(), TestCompilationFactory.GetMetadataReferences() )
                .WithService( SyntaxGenerationOptions.Formatted );

            serviceProvider = serviceProvider.WithCompileTimeProjectServices( CompileTimeProjectRepository.CreateTestInstance() );

            ((LinkerTestTextResult) testResult).ServiceProvider = serviceProvider;

            await base.RunAsync( testInput, testResult, testContext );

            if ( !testResult.Success )
            {
                return;
            }

            var builder = ((LinkerTestTextResult) testResult).Builder;

            CompilationModel initialCompilationModel = CompilationModel.CreateInitialInstance(
                new ProjectModel( testResult.InputCompilation, serviceProvider ),
                testResult.InputCompilation );

            // Create linker input from the clean compilation and recorded transformations.
            var linkerInput = builder.ToAspectLinkerInput( initialCompilationModel );

            var linker = new AspectLinker( serviceProvider, linkerInput );

            var result = await linker.ExecuteAsync( CancellationToken.None );

            var linkedCompilation = result.Compilation;

            testResult.OutputCompilation = linkedCompilation.Compilation;
            testResult.HasOutputCode = true;

            await testResult.SetOutputCompilationAsync( linkedCompilation.Compilation );

            // Attempt to Emit the result.
            var emitResult = linkedCompilation.Compilation.Emit( Stream.Null );

            testResult.PipelineDiagnostics.Report( emitResult.Diagnostics );

            if ( !emitResult.Success )
            {
                testResult.SetFailed( "Final Compilation.Emit failed." );
            }

            if ( !SyntaxTreeStructureVerifier.Verify( linkedCompilation.Compilation, out var diagnostics ) )
            {
                testResult.SetFailed( "Syntax tree verification failed." );
                testResult.OutputCompilationDiagnostics.Report( diagnostics );
            }
        }

        protected override void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            var assertionWalker = new LinkerInlineAssertionWalker();

            foreach ( var syntaxTree in testResult.SyntaxTrees )
            {
                assertionWalker.Visit( syntaxTree.OutputRunTimeSyntaxRoot );
            }

            base.ExecuteAssertions( testInput, testResult );
        }

        private protected override Compilation PreprocessCompilation( Compilation initialCompilation, TestResult testResult )
        {
            var serviceProvider = ((LinkerTestTextResult) testResult).ServiceProvider.AssertNotNull();
            var initialPartialCompilation = PartialCompilation.CreateComplete( initialCompilation );
            var builder = new LinkerTestInputBuilder( serviceProvider, initialPartialCompilation.CompilationContext );
            ((LinkerTestTextResult) testResult).Builder = builder;

            // Create a clean compilation without any pseudo members.
            var cleanCompilation = LinkerTestInputBuilder.GetCleanCompilation( initialPartialCompilation );

            // Process all syntax trees.
            var cleanPartialCompilation =
                initialPartialCompilation.UpdateSyntaxTrees(
                    syntaxTree =>
                        syntaxTree.WithRootAndOptions(
                            builder.ProcessSyntaxRoot( syntaxTree.GetRoot() ),
                            syntaxTree.Options ) );

            // It's important to return a compilation that does not contain any pseudo members and will be used as the initial compilation for the test.
            // It does, however, contain annotations that are used to create transformations.
            return cleanPartialCompilation.Compilation;
        }

        protected override TestResult CreateTestResult() => new LinkerTestTextResult();

        private sealed class LinkerTestTextResult : TestResult
        {
            public LinkerTestInputBuilder? Builder { get; set; }

            public ProjectServiceProvider? ServiceProvider { get; set; }
        }
    }
}