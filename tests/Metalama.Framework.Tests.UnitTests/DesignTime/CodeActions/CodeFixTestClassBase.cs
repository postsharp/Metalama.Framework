// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Telemetry;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rider;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Metalama.Framework.DesignTime.Utilities;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

public abstract class CodeFixTestClassBase : UnitTestClass
{
    private readonly TestExceptionReporter _exceptionReporter;

    public CodeFixTestClassBase()
    {
        this._exceptionReporter = new TestExceptionReporter();
    }

    public override void Dispose()
    {
        Assert.Empty( this._exceptionReporter.ReportedExceptions );
    }

    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        ((AdditionalServiceCollection) services).BackstageServices.Add( this._exceptionReporter );
        services.AddGlobalService( provider => new DesignTimeExceptionHandler( provider ) );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
        services.AddGlobalService<IUserDiagnosticRegistrationService>( new TestUserDiagnosticRegistrationService( shouldWrapUnsupportedDiagnostics: true ) );
    }

    private protected static async Task<(ImmutableArray<Diagnostic> diagnostics, GlobalServiceProvider serviceProvider)> ExecutePipeline(
        TestContext testContext, TestWorkspaceProvider workspace, TestDesignTimeAspectPipelineFactory pipelineFactory )
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

    private protected static async Task<(TestCodeFixContext codeFixContext, TestCodeRefactoringContext riderRefactoringContext)> QueryCodeFixes(
        TestWorkspaceProvider workspace, GlobalServiceProvider serviceProvider, ImmutableArray<Diagnostic> diagnostics, string targetTokenText )
    {
        var document = workspace.GetDocument( "target", "code.cs" );
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var span = syntaxRoot.DescendantTokens().Single( t => t.Text == targetTokenText ).Span;

        return await QueryCodeFixes( workspace, serviceProvider, diagnostics, span );
    }

    private protected static async Task<(TestCodeFixContext codeFixContext, TestCodeRefactoringContext riderRefactoringContext)> QueryCodeFixes(
        TestWorkspaceProvider workspace, GlobalServiceProvider serviceProvider, ImmutableArray<Diagnostic> diagnostics, TextSpan span)
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

    private class TestExceptionReporter : IExceptionReporter
    {
        private readonly ConcurrentBag<Exception> _reportedExceptions = new ConcurrentBag<Exception>();

        public IReadOnlyCollection<Exception> ReportedExceptions => this._reportedExceptions;

        public void ReportException( Exception reportedException, ExceptionReportingKind exceptionReportingKind = ExceptionReportingKind.Exception, string? localReportPath = null, IExceptionAdapter? exceptionAdapter = null )
        {
            this._reportedExceptions.Add( reportedException );
        }
    }
}