// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal partial class DesignTimeFallbackTestRunner : BaseTestRunner
    {
        public DesignTimeFallbackTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

            using var domain = new UnloadableCompileTimeDomain();

            var projectOptions = testResult.ProjectScopedServiceProvider.GetService<IProjectOptions>();

            // Replace the project options to enable design time fallback.
            var designTimeFallbackServiceProvider =
                testResult.ProjectScopedServiceProvider.WithService( new DesignTimeFallbackProjectOptions( projectOptions ) );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                designTimeFallbackServiceProvider,
                true,
                domain,
                AspectExecutionScenario.CompileTime );

            var inputCompilation = testResult.InputCompilation!;

            // Create a compilation from the input compilation while removing nodes marked for removal.
            foreach ( var inputSyntaxTree in inputCompilation!.SyntaxTrees )
            {
                inputCompilation = inputCompilation.ReplaceSyntaxTree(
                    inputSyntaxTree,
                    inputSyntaxTree.WithRootAndOptions( RemovingRewriter.Instance.Visit( inputSyntaxTree.GetRoot() ), inputSyntaxTree.Options ) );
            }

            var compileTimeResult = await compileTimePipeline.ExecuteAsync( testResult.PipelineDiagnostics, inputCompilation, default, CancellationToken.None );

            if ( compileTimeResult == null )
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed" );

                return;
            }

            // Create a compilation from the input compilation with removed nodes plus auxiliary files.
            var resultingCompilation = inputCompilation;

            var newSyntaxTrees = new List<SyntaxTree>();

            foreach ( var file in compileTimeResult.AuxiliaryFiles.Where(
                f => f.Kind == AuxiliaryFileKind.DesignTimeFallback && Path.GetExtension( f.Path ) == ".cs" ) )
            {
                var syntaxTree = CSharpSyntaxTree.ParseText( SourceText.From( file.Content, file.Content.Length ) );
                newSyntaxTrees.Add( syntaxTree );
                resultingCompilation = resultingCompilation.AddSyntaxTrees( syntaxTree );
            }

            if ( newSyntaxTrees.Count == 0 )
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute returned no design time fallback source files." );

                return;
            }

            testResult.InputCompilationDiagnostics.Report( resultingCompilation.GetDiagnostics() );
            testResult.HasOutputCode = true;

            // TODO: Multiple syntax trees.
            await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( newSyntaxTrees.Single().GetRoot() );

            var emitResult = resultingCompilation.Emit( Stream.Null );

            testResult.PipelineDiagnostics.Report( emitResult.Diagnostics );

            if ( !emitResult.Success )
            {
                testResult.SetFailed( "Final Compilation.Emit failed." );
            }
        }
    }
}