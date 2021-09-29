// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.Integration.Runners.Linker;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class LinkerTestRunner : BaseTestRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkerTestRunner"/> class.
        /// </summary>
        public LinkerTestRunner(
            IServiceProvider serviceProvider,
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
        private protected override async Task RunAsync( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            var builder = new LinkerTestInputBuilder(this.ServiceProvider);

            state["builder"] = builder;

            await base.RunAsync( testInput, testResult, state );

            if ( !testResult.Success )
            {
                return;
            }

            // Create the linker input.
            var linkerInput = builder.ToAspectLinkerInput( PartialCompilation.CreateComplete( testResult.InputCompilation.AssertNotNull() ) );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
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

        private protected override void ExecuteAssertions( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
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