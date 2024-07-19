// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD200

namespace Metalama.Framework.Tests.UnitTests.DesignTime.EndToEnd;

public sealed class VsCodeRefactoringProviderTests : DistributedDesignTimeTestBase
{
    [Fact]
    public async Task EndToEnd()
    {
        var analysisProcessServices = new ServiceProviderBuilder<IGlobalService>();
        analysisProcessServices.Add( sp => new CodeRefactoringDiscoveryService( sp ) );

        // Initialize the components.
        using var testContext = this.CreateDistributedDesignTimeTestContext( null, analysisProcessServices, null );

        await testContext.WhenFullyInitialized;

        const string code = """
                            using Metalama.Framework.Advising;
                            using Metalama.Framework.Aspects; 
                            using Metalama.Framework.Code;

                            class TheAspect : TypeAspect { }

                            class TheClass {}
                            """;

        // Initialize the workspace.
        var projectKey = testContext.WorkspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );
        await testContext.AnalysisProcessEndpoint.RegisterProjectAsync( projectKey );

        // Initialize the pipeline. It needs to execute before we ask for refactorings because the refactoring service uses the last results.
        var project = testContext.WorkspaceProvider.GetProject( "project" );
        var pipeline = testContext.PipelineFactory.GetOrCreatePipeline( project )!;
        await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );

        // Query refactorings.
        var document = testContext.WorkspaceProvider.GetDocument( "project", "code.cs" );

        var syntaxRoot = await document.GetSyntaxRootAsync();
        var syntaxNode = syntaxRoot!.DescendantTokens().Single( t => t.Text == "TheClass" );

        var codeRefactoringProvider = new TheCodeRefactoringProvider( testContext.UserProcessServiceProvider );
        var codeRefactoringContext = new TestCodeRefactoringContext( document, syntaxNode.Span );
        await codeRefactoringProvider.ComputeRefactoringsAsync( codeRefactoringContext );

        Assert.Single( codeRefactoringContext.RegisteredRefactorings );
    }
}