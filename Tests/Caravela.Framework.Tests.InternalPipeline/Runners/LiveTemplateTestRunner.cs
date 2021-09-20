// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.DesignTime.Refactoring;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Runners
{
    public class LiveTemplateTestRunner : BaseTestRunner
    {
        public LiveTemplateTestRunner( IServiceProvider serviceProvider, string? projectDirectory, MetadataReference[] metadataReferences, ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        private protected override async Task<TestResult> RunAsync( TestInput testInput, Dictionary<string, object?> state )
        {
            var testResult = await base.RunAsync( testInput, state );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            var compilation = CompilationModel.CreateInitialInstance( testResult.InputCompilation! );

            using var designTimePipeline = new DesignTimeAspectPipeline( buildOptions, domain, true );

            designTimePipeline.TryGetConfiguration(
                PartialCompilation.CreateComplete( testResult.InputCompilation! ),
                NullDiagnosticAdder.Instance,
                true,
                CancellationToken.None,
                out var configuration );

            var partialCompilation = PartialCompilation.CreateComplete( testResult.InputCompilation! );
            var target = compilation.DeclaredTypes.OfName( "TargetClass" ).Single().Methods.OfName( "TargetMethod" ).Single().GetSymbol();
            var aspectClass = designTimePipeline.AspectClasses!.Single( a => a.DisplayName == "TestAspect" );
            
            var success = LiveTemplateAspectPipeline.TryExecute(
                buildOptions,
                domain,
                configuration!,
                aspectClass,
                partialCompilation,
                target!,
                CancellationToken.None, 
                out var outputCompilation,
                out var diagnostics );

            testResult.PipelineDiagnostics.Report( diagnostics );

            if ( success )
            {
                testResult.HasOutputCode = true;

                var formattedOutputCompilation = await OutputCodeFormatter.FormatToSyntaxAsync( outputCompilation!, CancellationToken.None );
                
                var transformedSyntaxTree = formattedOutputCompilation.Compilation.SyntaxTrees.SingleOrDefault();

                var transformedSyntaxRoot = transformedSyntaxTree == null
                    ? SyntaxFactory.GlobalStatement( SyntaxFactoryEx.EmptyStatement )
                    : await transformedSyntaxTree.GetRootAsync();

                await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( transformedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "LiveTemplateAspectPipeline.TryExecute failed." );
            }

            return testResult;
        }
    }
}