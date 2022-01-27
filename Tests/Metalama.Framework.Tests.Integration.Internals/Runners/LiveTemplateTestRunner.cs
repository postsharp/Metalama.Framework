// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Templating;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    public class LiveTemplateTestRunner : BaseTestRunner
    {
        public LiveTemplateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

            using var domain = new UnloadableCompileTimeDomain();
            var serviceProvider = testResult.ProjectScopedServiceProvider;
            var compilation = CompilationModel.CreateInitialInstance( new NullProject( serviceProvider ), testResult.InputCompilation! );

            var partialCompilation = PartialCompilation.CreateComplete( testResult.InputCompilation! );
            var target = compilation.Types.OfName( "TargetClass" ).Single().Methods.OfName( "TargetMethod" ).Single().GetSymbol();

            var success = LiveTemplateAspectPipeline.TryExecute(
                serviceProvider,
                domain,
                null,
                c => c.BoundAspectClasses.Single<IAspectClass>( a => a.ShortName == "TestAspect" ),
                partialCompilation,
                target!,
                testResult.PipelineDiagnostics,
                CancellationToken.None,
                out var outputCompilation );

            if ( success )
            {
                testResult.HasOutputCode = true;

                var formattedOutputCompilation = await OutputCodeFormatter.FormatToSyntaxAsync( outputCompilation!, CancellationToken.None );

                var transformedSyntaxTree = formattedOutputCompilation.Compilation.SyntaxTrees.FirstOrDefault();

                var transformedSyntaxRoot = transformedSyntaxTree == null
                    ? SyntaxFactory.GlobalStatement( SyntaxFactoryEx.EmptyStatement )
                    : await transformedSyntaxTree.GetRootAsync();

                await testResult.SyntaxTrees.Single().SetRunTimeCodeAsync( transformedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "LiveTemplateAspectPipeline.TryExecute failed." );
            }
        }
    }
}