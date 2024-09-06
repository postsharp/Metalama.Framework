// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rider;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

#pragma warning disable VSTHRD200, CA1307

public sealed class CodeFixTests : CodeFixTestClassBase
{
    [Fact]
    public async Task RiderHiddenDiagnosticCodeFixTest()
    {
        using var testContext = this.CreateTestContext();

        const string code =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 
            using Metalama.Framework.Code;
            using Metalama.Framework.CodeFixes;
            using Metalama.Framework.Diagnostics;
            using System;

            internal class Aspect : MethodAspect
            {
                private static DiagnosticDefinition _diagHidden = new( "MY001", Severity.Hidden, "Add Hidden attribute" );
                private static DiagnosticDefinition _diagInfo = new( "MY002", Severity.Info, "Add Info attribute" );
            
                public override void BuildAspect( IAspectBuilder<IMethod> builder )
                {
                    base.BuildAspect( builder );
            
                    builder.Diagnostics.Report( _diagHidden.WithCodeFixes( CodeFixFactory.AddAttribute( builder.Target, typeof( HiddenAttribute ) ) ) );
            
                    builder.Diagnostics.Report( _diagInfo.WithCodeFixes( CodeFixFactory.AddAttribute( builder.Target, typeof( InfoAttribute ) ) ) );
            
                    builder.Diagnostics.Suggest( CodeFixFactory.AddAttribute( builder.Target, typeof( RefactoringAttribute ) ) );
                }
            }

            internal class HiddenAttribute : Attribute { }
            internal class InfoAttribute : Attribute { }
            internal class RefactoringAttribute : Attribute { }

            internal class TargetCode
            {
                [Aspect]
                public static int Method( int a )
                {
                    return a;
                }
            }
            """;

        // Initialize the workspace.
        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        workspace.AddOrUpdateProject( "target", new Dictionary<string, string> { ["code.cs"] = code } );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );

        var (diagnostics, serviceProvider) = await ExecutePipeline( testContext, workspace, factory );

        Assert.Equal( 3, diagnostics.Length );
        Assert.Single( diagnostics, d => d.Id == "MY001" );
        Assert.Single( diagnostics, d => d.Id == "MY002" );
        Assert.Single( diagnostics, d => d.Id == "LAMA0043" );

        // Query code fixes and refactorings.
        var (codeFixContext, codeRefactoringContext) = await QueryCodeFixes( workspace, serviceProvider, diagnostics, "Method" );

        Assert.Single( codeFixContext.RegisteredCodeFixes );
        Assert.Single( codeFixContext.RegisteredCodeFixes, fix => fix.CodeAction.Title.Contains( "[Info]" ) );

        Assert.Equal( 2, codeRefactoringContext.RegisteredRefactorings.Count );
        Assert.Single( codeRefactoringContext.RegisteredRefactorings, refactoring => refactoring.Title.Contains( "[Hidden]" ) );
        Assert.Single( codeRefactoringContext.RegisteredRefactorings, refactoring => refactoring.Title.Contains( "[Refactoring]" ) );
    }

    [Fact( Skip = "https://postsharp.tpondemand.com/entity/33620-code-fixes-tests-fail-intermittently" )]
    public async Task CodeFixesInDependency()
    {
        using var testContext = this.CreateTestContext();

        const string libraryCode =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 
            using Metalama.Framework.Code;
            using Metalama.Framework.CodeFixes;
            using Metalama.Framework.Diagnostics;
            using System;
            using System.Linq;

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class RequiredAttribute : Attribute { }

            [RequiresAttribute]
            public interface IRequiresAttribute { }

            [Inheritable]
            public sealed class RequiresAttribute : TypeAspect
            {
                private static readonly DiagnosticDefinition<INamedType> typeNeedsAttribute = new(
                    "RA01",
                    Severity.Error,
                    "The class '{0}' must be annotated with [RequiredAttribute].");
                
                public override void BuildAspect(IAspectBuilder<INamedType> builder)
                {
                    bool hasRequiredAttribute = builder.Target.Attributes.Any(a => a.Type.Is(typeof(RequiredAttribute)));
                    if (!hasRequiredAttribute)
                    {
                        builder.Diagnostics.Report(typeNeedsAttribute.WithArguments(builder.Target), builder.Target);
                        builder.Diagnostics.Suggest(CodeFixFactory.AddAttribute(builder.Target, typeof(RequiredAttribute)), builder.Target);
                    }
                }
            }
            """;

        const string appCode =
            """
            namespace NS
            {
                /// <summary>
                /// Some doc.
                /// </summary>
                public sealed class TestClass : IRequiresAttribute
                {
                }
            }
            """;

        const string modifiedAppCode =
            """
            namespace NS
            {
                /// <summary>
                /// Some doc.
                /// </summary>
                [Required]
                public sealed class TestClass : IRequiresAttribute
                {
                }
            }
            """;

        // Initialize the workspace.
        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        workspace.AddOrUpdateProject( "library", new Dictionary<string, string> { ["library-code.cs"] = libraryCode } );
        workspace.AddOrUpdateProject( "target", new Dictionary<string, string> { ["code.cs"] = appCode }, ["library"] );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );

        var (diagnostics, serviceProvider) = await ExecutePipeline( testContext, workspace, factory );

        Assert.Equal( 2, diagnostics.Length );
        Assert.Single( diagnostics, d => d.Id == "RA01" );
        Assert.Single( diagnostics, d => d.Id == "LAMA0043" );

        // Query code fixes.
        var (codeFixContext, _) = await QueryCodeFixes( workspace, serviceProvider, diagnostics, "TestClass" );

        var codeFix = Assert.Single( codeFixContext.RegisteredCodeFixes );
        Assert.Contains( "[Required]", codeFix.CodeAction.Title );

        // Apply code fix.
        var previewOperation = Assert.Single( await codeFix.CodeAction.GetPreviewOperationsAsync( testContext.CancellationToken ) );

        previewOperation.Apply( workspace.Workspace, testContext.CancellationToken );

        var modifiedText = await workspace.GetProject( "target" ).Documents.Single().GetTextAsync();

        AssertEx.EolInvariantEqual( modifiedAppCode, modifiedText.ToString() );
    }

    [Fact]
    public async Task FabricCodeFix()
    {
        using var testContext = this.CreateTestContext();

        const string fabricCode = """
            using Metalama.Framework.CodeFixes;
            using Metalama.Framework.Diagnostics;
            using Metalama.Framework.Fabrics;

            class A : Attribute;

            class Fabric : TransitiveProjectFabric
            {
                private static DiagnosticDefinition _diag = new("TEST01", Severity.Warning, "warning from fabric");

                public override void AmendProject(IProjectAmender amender)
                {
                    amender
                        .SelectMany(project => project.Types)
                        .SelectMany(type => type.Properties)
                        .ReportDiagnostic(prop => _diag.WithCodeFixes(CodeFixFactory.AddAttribute(prop, typeof(A))));
                }
            }
            """;

        const string targetCode = """
            class C
            {
                string? Name { get; set; }
            }
            """;

        const string modifiedTargetCode = """
            class C
            {
                [global::A]
                string? Name { get; set; }
            }
            """;

        // Initialize the workspace.
        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        workspace.AddOrUpdateProject( "fabric", new Dictionary<string, string> { ["fabric.cs"] = fabricCode } );
        workspace.AddOrUpdateProject( "target", new Dictionary<string, string> { ["code.cs"] = targetCode }, ["fabric"] );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );

        var (diagnostics, serviceProvider) = await ExecutePipeline( testContext, workspace, factory );

        Assert.Single( diagnostics );
        Assert.Single( diagnostics, d => d.Id == "TEST01" );

        // Query code fixes.
        var (codeFixContext, _) = await QueryCodeFixes( workspace, serviceProvider, diagnostics, "Name" );

        var codeFix = Assert.Single( codeFixContext.RegisteredCodeFixes );
        Assert.Contains( "[A]", codeFix.CodeAction.Title );

        // Apply code fix.
        var previewOperation = Assert.Single( await codeFix.CodeAction.GetPreviewOperationsAsync( testContext.CancellationToken ) );

        previewOperation.Apply( workspace.Workspace, testContext.CancellationToken );

        var modifiedText = await workspace.GetProject( "target" ).Documents.Single().GetTextAsync();

        AssertEx.EolInvariantEqual( modifiedTargetCode, modifiedText.ToString() );
    }
}