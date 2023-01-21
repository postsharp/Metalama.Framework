// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.EndToEnd;

#pragma warning disable VSTHRD200

public sealed class DiagnosticAnalyzerTests : UnitTestClass
{
    private async Task<List<Diagnostic>> RunAnalyzer( string code )
    {
        using var testContext = this.CreateTestContext();

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var workspaceProvider = new TestWorkspaceProvider( testContext.ServiceProvider );
        workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string>() { ["code.cs"] = code } );
        var compilation = await workspaceProvider.GetProject( "project" ).GetCompilationAsync();
        var syntaxTree = await workspaceProvider.GetDocument( "project", "code.cs" ).GetSyntaxTreeAsync();
        var semanticModel = compilation!.GetSemanticModel( syntaxTree! );

        var analyzer = new TheDiagnosticAnalyzer( pipelineFactory.ServiceProvider );
        var analysisContext = new TestSemanticModelAnalysisContext( semanticModel, testContext.ProjectOptions );

        analyzer.AnalyzeSemanticModel( analysisContext );

        return analysisContext.ReportedDiagnostics;
    }

    [Fact]
    public async Task Nothing()
    {
        var diagnostics = await this.RunAnalyzer( "" );
        Assert.Empty( diagnostics );
    }

    [Fact]
    public async Task CSharpErrorIsNotReported()
    {
        const string code = """
using Metalama.Framework.Aspects;

class TheAspect : TypeAspect
{
   SomeError;
}

""";

        var diagnostics = await this.RunAnalyzer( code );

        // CSharp errors should not be reported at design time.
        Assert.Empty( diagnostics );
    }

    [Fact]
    public async Task CompileTimeMetalamaDiagnosticsAreReported()
    {
        const string code = """
using Metalama.Framework.Aspects;

class TheAspect : OverrideMethodAspect 
{ 
   public override dynamic? OverrideMethod()
   {
      // The following line is an error.
      meta.InsertComment( meta.Proceed() );
   }
}


""";

        var diagnostics = await this.RunAnalyzer( code );

        // CSharp errors should not be reported at design time.
        Assert.Equal( TemplatingDiagnosticDescriptors.ScopeMismatch.Id, Assert.Single( diagnostics ).Id );
    }
}