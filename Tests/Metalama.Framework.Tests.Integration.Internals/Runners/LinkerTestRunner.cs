// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Linking;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Impl.Testing;
using Metalama.Framework.Tests.Integration.Runners.Linker;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class LinkerTestRunner : BaseTestRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkerTestRunner"/> class.
        /// </summary>
        public LinkerTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base(
                serviceProvider,
                projectDirectory,
                metadataReferences,
                logger ) { }

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
            // There is a chicken-or-test in the design of the test because the project-scoped service provider is needed before the compilation
            // is created. We break the cycle by providing the service provider with the default set of references, which should work for 
            // the linker tests because they are not cross-assembly.
            var preliminaryProjectBuilder = this.BaseServiceProvider.WithProjectScopedServices( TestCompilationFactory.GetMetadataReferences() );

            var builder = new LinkerTestInputBuilder( preliminaryProjectBuilder );

            state["builder"] = builder;

            await base.RunAsync( testInput, testResult, state );

            if ( !testResult.Success )
            {
                return;
            }

            // Create the linker input.
            var linkerInput = builder.ToAspectLinkerInput( PartialCompilation.CreateComplete( testResult.InputCompilation.AssertNotNull() ) );
            var linker = new AspectLinker( testResult.ProjectScopedServiceProvider, linkerInput );
            var result = linker.ToResult();

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

        private protected override SyntaxNode PreprocessSyntaxRoot( TestInput testInput, SyntaxNode syntaxRoot, Dictionary<string, object?> state )
        {
            var builder = (LinkerTestInputBuilder) state["builder"]!;

            return builder.ProcessSyntaxRoot( syntaxRoot );
        }
    }
}