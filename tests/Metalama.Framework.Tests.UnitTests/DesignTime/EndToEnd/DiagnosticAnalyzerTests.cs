// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.Engine.Diagnostics;
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

    [Fact]
    public async Task UserError()
    {
        var getCode = (string extraCode) => $$"""
            using Metalama.Framework.Aspects;
            using Metalama.Framework.Code;
            using Metalama.Framework.Diagnostics;

            class ErrorAspect : TypeAspect
            {
                static readonly DiagnosticDefinition _error = new( "MLTEST", Severity.Error, "Error!" );

                public override void BuildAspect( IAspectBuilder<INamedType> builder )
                {
                    builder.Diagnostics.Report( _error );
                    {{extraCode}}
                }
            }
            
            [ErrorAspect]
            class C {}
            """;

        using var testContext = this.CreateTestContext();

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var workspaceProvider = new TestWorkspaceProvider( testContext.ServiceProvider );
        workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string>() { ["code.cs"] = getCode( "" ) } );
        var compilation1 = await workspaceProvider.GetProject( "project" ).GetCompilationAsync();
        var syntaxTree1 = await workspaceProvider.GetDocument( "project", "code.cs" ).GetSyntaxTreeAsync();
        var semanticModel1 = compilation1!.GetSemanticModel( syntaxTree1! );

        var analyzer = new TheDiagnosticAnalyzer( pipelineFactory.ServiceProvider );

        var analysisContext1 = new TestSemanticModelAnalysisContext( semanticModel1, testContext.ProjectOptions );
        analyzer.AnalyzeSemanticModel( analysisContext1 );
        var diagnostic1 = Assert.Single( analysisContext1.ReportedDiagnostics );

        Assert.Equal( string.Format( DesignTimeDiagnosticDescriptors.UserError.MessageFormat, "MLTEST", "Error!" ), diagnostic1.GetLocalizedMessage() );

        workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string>() { ["code.cs"] = getCode( "// whatever" ) } );
        var compilation2 = await workspaceProvider.GetProject( "project" ).GetCompilationAsync();
        var syntaxTree2 = await workspaceProvider.GetDocument( "project", "code.cs" ).GetSyntaxTreeAsync();
        var semanticModel2 = compilation2!.GetSemanticModel( syntaxTree2! );

        var analysisContext2 = new TestSemanticModelAnalysisContext( semanticModel2, testContext.ProjectOptions );
        analyzer.AnalyzeSemanticModel( analysisContext2 );
        var diagnostic2 = Assert.Single( analysisContext2.ReportedDiagnostics );

        Assert.Equal( string.Format( DesignTimeDiagnosticDescriptors.UserError.MessageFormat, "MLTEST", "Error!" ), diagnostic2.GetLocalizedMessage() );

        Assert.Equal( diagnostic1.Id, diagnostic2.Id );
        Assert.Equal( diagnostic1.Descriptor.Id, diagnostic2.Descriptor.Id );
        Assert.Equal( diagnostic1.Descriptor.Title, diagnostic2.Descriptor.Title );
        Assert.Equal( diagnostic1.Descriptor.HelpLinkUri, diagnostic2.Descriptor.HelpLinkUri );
        Assert.Equal( diagnostic1.Descriptor.MessageFormat.ToString(), diagnostic2.Descriptor.MessageFormat.ToString() );
        Assert.Equal( diagnostic1.Descriptor.Category, diagnostic2.Descriptor.Category );
        Assert.Equal( diagnostic1.Descriptor.DefaultSeverity, diagnostic2.Descriptor.DefaultSeverity );
        Assert.Equal( diagnostic1.Descriptor.IsEnabledByDefault, diagnostic2.Descriptor.IsEnabledByDefault );
        Assert.Equal( diagnostic1.Descriptor.CustomTags, diagnostic2.Descriptor.CustomTags );
        Assert.Equal( diagnostic1.Severity, diagnostic2.Severity );
        Assert.Equal( diagnostic1.DefaultSeverity, diagnostic2.DefaultSeverity );
        Assert.Equal( diagnostic1.WarningLevel, diagnostic2.WarningLevel );
        Assert.Equal( diagnostic1.Properties, diagnostic2.Properties );
    }
}