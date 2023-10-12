// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rider;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

#pragma warning disable VSTHRD200, CA1307

public sealed class CodeFixTests : UnitTestClass
{
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
        services.AddGlobalService<IUserDiagnosticRegistrationService>( new TestUserDiagnosticRegistrationService() );
    }

    [Fact]
    public async Task RiderHiddenDiagnosticCodeFixTest()
    {
        using var testContext = this.CreateTestContext();

        const string code = """
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
        workspace.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );
        var serviceProvider = testContext.ServiceProvider.Global.WithService( factory );

        serviceProvider = serviceProvider.WithServices(
            new CodeActionExecutionService( serviceProvider ),
            new CodeRefactoringDiscoveryService( serviceProvider ) );

        var project = workspace.GetProject( "project" );
        var pipeline = factory.GetOrCreatePipeline( project )!;
        var result = await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );
        Assert.True( result.IsSuccessful );
        var diagnostics = result.Value.GetDiagnosticsOnSyntaxTree( "code.cs" ).Diagnostics;
        Assert.Equal( 3, diagnostics.Length );
        Assert.Single( diagnostics, d => d.Id == "MY001" );
        Assert.Single( diagnostics, d => d.Id == "MY002" );
        Assert.Single( diagnostics, d => d.Id == "LAMA0043" );

        // Query code fixes.
        var document = workspace.GetDocument( "project", "code.cs" );
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var token = syntaxRoot!.DescendantTokens().Single( t => t.Text == "Method" );

        var codeFixProvider = new RiderCodeFixProvider( serviceProvider );
        var codeFixContext = new TestCodeFixContext( document, diagnostics, token.Span );

        await codeFixProvider.RegisterCodeFixesAsync( codeFixContext );

        Assert.Single( codeFixContext.RegisteredCodeFixes );
        Assert.Single( codeFixContext.RegisteredCodeFixes, fix => fix.CodeAction.Title.Contains( "[Info]" ) );

        // Query refactorings.
        var codeRefactoringProvider = new RiderCodeRefactoringProvider( serviceProvider );
        var codeRefactoringContext = new TestCodeRefactoringContext( document, token.Span );

        await codeRefactoringProvider.ComputeRefactoringsAsync( codeRefactoringContext );

        Assert.Equal( 2, codeRefactoringContext.RegisteredRefactorings.Count );
        Assert.Single( codeRefactoringContext.RegisteredRefactorings, refactoring => refactoring.Title.Contains( "[Hidden]" ) );
        Assert.Single( codeRefactoringContext.RegisteredRefactorings, refactoring => refactoring.Title.Contains( "[Refactoring]" ) );
    }

    [Fact( Skip = "https://postsharp.tpondemand.com/entity/33620-code-fixes-tests-fail-intermittently" )]
    public async Task CodeFixesInDependency()
    {
        using var testContext = this.CreateTestContext();

        const string libraryCode = """
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

        const string appCode = """
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

        const string modifiedAppCode = """
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
        workspace.AddOrUpdateProject( "app", new Dictionary<string, string> { ["code.cs"] = appCode }, new[] { "library" } );

        // Execute the pipeline to get diagnostics.
        using var factory = new TestDesignTimeAspectPipelineFactory( testContext );
        var serviceProvider = testContext.ServiceProvider.Global.WithService( factory );

        serviceProvider = serviceProvider.WithServices(
            new CodeActionExecutionService( serviceProvider ),
            new CodeRefactoringDiscoveryService( serviceProvider ) );

        var project = workspace.GetProject( "app" );
        var pipeline = factory.GetOrCreatePipeline( project )!;
        var result = await pipeline.ExecuteAsync( (await project.GetCompilationAsync())!, AsyncExecutionContext.Get() );
        Assert.True( result.IsSuccessful );
        var diagnostics = result.Value.GetDiagnosticsOnSyntaxTree( "code.cs" ).Diagnostics;
        Assert.Equal( 2, diagnostics.Length );
        Assert.Single( diagnostics, d => d.Id == "RA01" );
        Assert.Single( diagnostics, d => d.Id == "LAMA0043" );

        // Query code fixes.
        var document = workspace.GetDocument( "app", "code.cs" );
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var token = syntaxRoot!.DescendantTokens().Single( t => t.Text == "TestClass" );

        var codeFixProvider = new TheCodeFixProvider( serviceProvider );
        var codeFixContext = new TestCodeFixContext( document, diagnostics, token.Span );

        await codeFixProvider.RegisterCodeFixesAsync( codeFixContext );

        var codeFix = Assert.Single( codeFixContext.RegisteredCodeFixes );
        Assert.Contains( "[Required]", codeFix.CodeAction.Title );

        // Apply code fix.
        var previewOperation = Assert.Single( await codeFix.CodeAction.GetPreviewOperationsAsync( testContext.CancellationToken ) );

        previewOperation.Apply( workspace.Workspace, testContext.CancellationToken );

        var modifiedText = await workspace.GetProject( "app" ).Documents.Single().GetTextAsync();

        Assert.Equal( modifiedAppCode, modifiedText.ToString() );
    }
}