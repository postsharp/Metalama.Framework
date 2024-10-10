// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rider;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

public abstract class CodeFixTestClassBase : FrameworkBaseTestClass
{
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
        services.AddGlobalService<IUserDiagnosticRegistrationService>( new TestUserDiagnosticRegistrationService( shouldWrapUnsupportedDiagnostics: true ) );
    }

    private protected static async Task<(ImmutableArray<Diagnostic> diagnostics, GlobalServiceProvider serviceProvider)> ExecutePipelineAsync(
        TestContext testContext,
        TestWorkspaceProvider workspace,
        TestDesignTimeAspectPipelineFactory pipelineFactory )
    {
        var serviceProvider = testContext.ServiceProvider.Global.WithService( pipelineFactory );

        serviceProvider = serviceProvider.WithServices(
            new CodeActionExecutionService( serviceProvider ),
            new CodeRefactoringDiscoveryService( serviceProvider ) );

        var project = workspace.GetProject( "target" );
        var pipeline = pipelineFactory.GetOrCreatePipeline( project )!;
        var result = await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );
        Assert.True( result.IsSuccessful );
        var diagnostics = result.Value.GetDiagnosticsOnSyntaxTree( "code.cs" );

        return (diagnostics, serviceProvider);
    }

    private protected static async Task<(TestCodeFixContext codeFixContext, TestCodeRefactoringContext riderRefactoringContext)> QueryCodeFixesAsync(
        TestWorkspaceProvider workspace,
        GlobalServiceProvider serviceProvider,
        ImmutableArray<Diagnostic> diagnostics,
        string targetTokenText )
    {
        var document = workspace.GetDocument( "target", "code.cs" );
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var span = syntaxRoot.DescendantTokens().Single( t => t.Text == targetTokenText ).Span;

        return await QueryCodeFixesAsync( workspace, serviceProvider, diagnostics, span );
    }

    private protected static async Task<(TestCodeFixContext codeFixContext, TestCodeRefactoringContext riderRefactoringContext)> QueryCodeFixesAsync(
        TestWorkspaceProvider workspace,
        GlobalServiceProvider serviceProvider,
        ImmutableArray<Diagnostic> diagnostics,
        TextSpan span )
    {
        var document = workspace.GetDocument( "target", "code.cs" );

        var codeFixProvider = new RiderCodeFixProvider( serviceProvider );
        var codeFixContext = new TestCodeFixContext( document, diagnostics, span );

        await codeFixProvider.RegisterCodeFixesAsync( codeFixContext );

        var codeRefactoringProvider = new RiderCodeRefactoringProvider( serviceProvider );
        var codeRefactoringContext = new TestCodeRefactoringContext( document, span );

        await codeRefactoringProvider.ComputeRefactoringsAsync( codeRefactoringContext );

        return (codeFixContext, codeRefactoringContext);
    }
}