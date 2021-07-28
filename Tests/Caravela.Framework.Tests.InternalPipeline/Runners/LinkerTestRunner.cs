// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.InternalPipeline.Runners.Linker;
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
                logger )
        { }

        /// <summary>
        /// Runs the template test with name and source provided in the <paramref name="testInput"/>.
        /// </summary>
        /// <param name="testInput">Specifies the input test parameters such as the name and the source.</param>
        /// <returns>The result of the test execution.</returns>
        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var builder = new LinkerTestInputBuilder();
            var testResult = await base.RunTestAsync( TestInputWithBuilder.Create( testInput, builder ) );

            if ( !testResult.Success )
            {
                return testResult;
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

            testResult.Report( emitResult.Diagnostics );

            if ( !emitResult.Success )
            {
                testResult.SetFailed( "Final Compilation.Emit failed." );
            }

            return testResult;
        }

        protected override SyntaxNode TransformSyntaxRoot( TestInput testInput, SyntaxNode syntaxRoot )
        {
            var builder = ((TestInputWithBuilder) testInput).Builder;

            return builder.ProcessSyntaxRoot( syntaxRoot );
        }

        private class TestInputWithBuilder : TestInput
        {
            public LinkerTestInputBuilder Builder { get; }

            private TestInputWithBuilder(
                string testName,
                string sourceCode,
                string? projectDirectory,
                string? relativePath,
                string? fullPath,
                TestOptions options,
                LinkerTestInputBuilder builder)
                : base( testName, sourceCode, projectDirectory, relativePath, fullPath, options )
            {
                this.Builder = builder;
            }

            public static TestInputWithBuilder Create( TestInput testInput, LinkerTestInputBuilder builder )
                => new TestInputWithBuilder( testInput.TestName, testInput.SourceCode, testInput.ProjectDirectory, testInput.RelativePath, testInput.FullPath, testInput.Options, builder );
        }
    }
}
