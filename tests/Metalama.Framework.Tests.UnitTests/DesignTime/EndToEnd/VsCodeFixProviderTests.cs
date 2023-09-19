// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.EndToEnd;

#pragma warning disable VSTHRD200

public sealed class VsCodeFixProviderTests : DistributedDesignTimeTestBase
{
    public VsCodeFixProviderTests( ITestOutputHelper? logger = null ) : base( logger ) { }

    [Fact]
    public async Task EndToEnd()
    {
        var analysisProcessServices = new ServiceProviderBuilder<IGlobalService>();

        // Initialize the components.
        using var testContext = this.CreateDistributedDesignTimeTestContext( null, analysisProcessServices, null );

        await testContext.WhenFullyInitialized;

        const string code = """
                            using Metalama.Framework.Aspects;
                            using Metalama.Framework.Advising;
                            using Metalama.Framework.Code;
                            using Metalama.Framework.CodeFixes;

                            class TheAspect : TypeAspect
                            {
                               [Introduce]
                               void IntroducedMethod(){}
                            
                               public override void BuildAspect( IAspectBuilder<INamedType> builder )
                               {
                                   base.BuildAspect( builder );
                                   builder.Diagnostics.Suggest( CodeFixFactory.AddAttribute( builder.Target, typeof(TheAspect) ), builder.Target );
                               }
                            }

                            [TheAspect
                            class TheClass {}
                            """;

        // Initialize the workspace.
        var projectKey = testContext.WorkspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );
        await testContext.AnalysisProcessEndpoint.RegisterProjectAsync( projectKey );

        // Execute the pipeline to get diagnostics.
        var project = testContext.WorkspaceProvider.GetProject( "project" );
        var pipeline = testContext.PipelineFactory.GetOrCreatePipeline( project )!;
        var result = await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );
        Assert.True( result.IsSuccessful );
        var diagnostics = result.Value.GetDiagnosticsOnSyntaxTree( "code.cs" ).Diagnostics;
        Assert.Single( diagnostics, d => d.Id == GeneralDiagnosticDescriptors.TypeNotPartial.Id );
        Assert.Single( diagnostics, d => d.Id == GeneralDiagnosticDescriptors.SuggestedCodeFix.Id );

        // Query code fixes.

        var document = testContext.WorkspaceProvider.GetDocument( "project", "code.cs" );
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var syntaxNode = syntaxRoot!.DescendantTokens().Single( t => t.Text == "TheClass" );

        var codeFixProvider = new TheCodeFixProvider( testContext.UserProcessServiceProvider );
        var codeFixContext = new TestCodeFixContext( document, diagnostics, syntaxNode.Span );

        await codeFixProvider.RegisterCodeFixesAsync( codeFixContext );

        Assert.Equal( 2, codeFixContext.RegisteredCodeFixes.Count );
    }
}