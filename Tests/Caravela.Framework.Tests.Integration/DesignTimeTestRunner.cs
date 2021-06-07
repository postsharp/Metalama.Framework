using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Tests.Integration.DesignTime
{
    class DesignTimeTestRunner : TestRunnerBase
    {
        public DesignTimeTestRunner( IServiceProvider serviceProvider, string? projectDirectory = null ) : base( serviceProvider, projectDirectory )
        {
            
        }
        
        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var testResult = await base.RunTestAsync( testInput );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new DesignTimeAspectPipeline( buildOptions, domain );
            var pipelineResult = pipeline.Execute( PartialCompilation.CreateComplete( testResult.InitialCompilation ), CancellationToken.None );

            testResult.Report( pipelineResult.Diagnostics.ReportedDiagnostics );

            if ( pipelineResult.Success )
            {
                var introducedSyntaxTree = pipelineResult.IntroducedSyntaxTrees.SingleOrDefault();
                testResult.SetTransformedTarget( introducedSyntaxTree?.GeneratedSyntaxTree.GetRoot()  ?? SyntaxFactoryEx.EmptyStatement );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            return testResult;
        }
    }
}