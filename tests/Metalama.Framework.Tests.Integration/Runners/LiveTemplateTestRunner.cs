// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting;
using Metalama.Testing.AspectTesting.Licensing;
using Metalama.Testing.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal sealed class LiveTemplateTestRunner : BaseTestRunner
    {
        public LiveTemplateTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger,
            ILicenseKeyProvider? licenseKeyProvider )
            : base( serviceProvider, projectDirectory, references, logger, licenseKeyProvider ) { }

        protected override async Task RunAsync(
            TestInput testInput,
            TestResult testResult,
            TestContext testContext )
        {
            Assert.True( testInput.Options.TestScenario is TestScenario.ApplyLiveTemplate or TestScenario.PreviewLiveTemplate );

            await base.RunAsync( testInput, testResult, testContext );

            var serviceProvider = testContext.ServiceProvider.AddLicenseConsumptionManagerForTest( testInput, this.LicenseKeyProvider );

            var compilation = CompilationModel.CreateInitialInstance(
                new ProjectModel( TestCompilationFactory.CreateEmptyCSharpCompilation( "test" ), serviceProvider ),
                testResult.InputCompilation! );

            var partialCompilation = PartialCompilation.CreateComplete( testResult.InputCompilation! );
            var targetMethod = compilation.Types.OfName( "TargetClass" ).Single().Methods.OfName( "TargetMethod" ).Single();
            var target = targetMethod.GetSymbol();

            var result = await LiveTemplateAspectPipeline.ExecuteAsync(
                serviceProvider,
                testContext.Domain,
                null,
                c => c.BoundAspectClasses.Single<IAspectClass>( a => a.ShortName == "TestAspect" ),
                partialCompilation,
                target!,
                testResult.PipelineDiagnostics,
                testInput.Options.TestScenario == TestScenario.PreviewLiveTemplate );

            if ( result.IsSuccessful )
            {
                testResult.HasOutputCode = true;

                var formattedOutputCompilation = await new CodeFormatter().FormatAsync( result.Value, CancellationToken.None );

                var targetSyntaxTree = targetMethod.GetPrimarySyntaxTree();

                var inputSyntaxTreeIndex = -1;
                TestSyntaxTree testSyntaxTree;

                do
                {
                    testSyntaxTree = testResult.SyntaxTrees.ElementAt( ++inputSyntaxTreeIndex );
                }
                while ( testSyntaxTree.InputSyntaxTree != targetSyntaxTree );

                var transformedSyntaxTree = formattedOutputCompilation.Compilation.SyntaxTrees.ElementAt( inputSyntaxTreeIndex );

                var transformedSyntaxRoot = await transformedSyntaxTree.GetRootAsync();

                await testSyntaxTree.SetRunTimeCodeAsync( transformedSyntaxRoot );
            }
            else
            {
                testResult.SetFailed( "LiveTemplateAspectPipeline.TryExecute failed." );
            }
        }
    }
}