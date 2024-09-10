// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

#pragma warning disable VSTHRD200, CA1307

public sealed class CodeFixIssueTests : CodeFixTestClassBase
{
    [Fact]
    public async Task AliasDefinitionDoesNotCauseError()
    {        
        using var testContext = this.CreateTestContext();
        const string code =
            """
            using myAlias = System;
            """;

        // Initialize the workspace.
        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        workspace.AddOrUpdateProject( "target", new Dictionary<string, string> { ["code.cs"] = code } );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );

        var (diagnostics, serviceProvider) = await ExecutePipelineAsync( testContext, workspace, factory );

        // Query code fixes and refactorings.
        var (codeFixContext, codeRefactoringContext) = await QueryCodeFixesAsync( workspace, serviceProvider, diagnostics, TextSpan.FromBounds(0, 1) );

        Assert.Empty( codeFixContext.RegisteredCodeFixes );
        Assert.Empty( codeRefactoringContext.RegisteredRefactorings );
    }

    [Fact]
    public async Task OutOfBoundsSpanDoesNotCauseError()
    {
        using var testContext = this.CreateTestContext();
        const string code =
            """
            void Foo() {}
            """;

        // Initialize the workspace.
        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        workspace.AddOrUpdateProject( "target", new Dictionary<string, string> { ["code.cs"] = code } );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );

        var (diagnostics, serviceProvider) = await ExecutePipelineAsync( testContext, workspace, factory );

        // Query code fixes and refactorings.
        var (codeFixContext, codeRefactoringContext) = await QueryCodeFixesAsync( workspace, serviceProvider, diagnostics, TextSpan.FromBounds( 100, 101 ) );

        Assert.Empty( codeFixContext.RegisteredCodeFixes );
        Assert.Empty( codeRefactoringContext.RegisteredRefactorings );
    }
}