// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Templating;
using Metalama.TestFramework;
using Metalama.TestFramework.Licensing;
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
            TestProjectReferences references,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, references, logger ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            Dictionary<string, object?> state )
        {
            await base.RunAsync( testInput, testResult, state );

            using var domain = new UnloadableCompileTimeDomain();
            var serviceProvider = testResult.ProjectScopedServiceProvider.AddLicenseVerifierForTest( testInput );
            var compilation = CompilationModel.CreateInitialInstance( new NullProject( serviceProvider ), testResult.InputCompilation! );

            var partialCompilation = PartialCompilation.CreateComplete( testResult.InputCompilation! );
            var targetMethod = compilation.Types.OfName( "TargetClass" ).Single().Methods.OfName( "TargetMethod" ).Single();
            var target = targetMethod.GetSymbol();

            var pipeline = new TestDesignTimeAspectPipeline( serviceProvider, domain );

            var result = await LiveTemplateAspectPipeline.ExecuteAsync(
                serviceProvider,
                domain,
                null,
                c => c.BoundAspectClasses.Single<IAspectClass>( a => a.ShortName == "TestAspect" ),
                partialCompilation,
                target!,
                testResult.PipelineDiagnostics,
                testInput.Options.PreviewLiveTemplate.GetValueOrDefault(),
                CancellationToken.None );

            if ( result.IsSuccessful )
            {
                testResult.HasOutputCode = true;

                var formattedOutputCompilation = await OutputCodeFormatter.FormatToSyntaxAsync( result.Value, CancellationToken.None );

                var targetSyntaxTree = targetMethod.GetPrimarySyntaxTree();

                var inputSyntaxTreeIndex = -1;
                TestSyntaxTree testSyntaxTree;

                do
                {
                    testSyntaxTree = testResult.SyntaxTrees.ElementAt( ++inputSyntaxTreeIndex );
                }
                while ( testSyntaxTree.InputSyntaxTree != targetSyntaxTree );

                var transformedSyntaxTree = formattedOutputCompilation.Compilation.SyntaxTrees.ElementAt( inputSyntaxTreeIndex );

                var transformedSyntaxRoot = transformedSyntaxTree == null
                    ? SyntaxFactory.GlobalStatement( SyntaxFactoryEx.EmptyStatement )
                    : await transformedSyntaxTree.GetRootAsync();

                await testSyntaxTree.SetRunTimeCodeAsync( transformedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "LiveTemplateAspectPipeline.TryExecute failed." );
            }
        }
    }
}