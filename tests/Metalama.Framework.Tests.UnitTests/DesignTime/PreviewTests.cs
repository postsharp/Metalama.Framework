﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Preview;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class PreviewTests : DesignTimeTestBase
{
    private const string _mainProjectName = "master";

    public PreviewTests( ITestOutputHelper logger ) : base( logger ) { }
    
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
    }

    protected override TestContextOptions GetDefaultTestContextOptions() => new TestContextOptions() { CodeFormattingOptions = CodeFormattingOptions.Formatted };

    private Task<string> RunPreviewAsync(
        Dictionary<string, string> code,
        string previewedSyntaxTreeName,
        Dictionary<string, string>? dependencyCode = null )
    {
        using var testContext = this.CreateTestContext();
        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        return RunPreviewAsync(
            testContext,
            testContext.ServiceProvider.Global.WithService( pipelineFactory ),
            code,
            previewedSyntaxTreeName,
            dependencyCode );
    }

    private static async Task<string> RunPreviewAsync(
        TestContext testContext,
        GlobalServiceProvider serviceProvider,
        Dictionary<string, string> code,
        string previewedSyntaxTreeName,
        Dictionary<string, string>? dependencyCode = null )
    {
        string[]? references;

        var workspace = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();

        if ( dependencyCode != null )
        {
            workspace.AddOrUpdateProject( "dependency", dependencyCode );
            references = new[] { "dependency" };
        }
        else
        {
            references = null;
        }

        var projectKey = workspace.AddOrUpdateProject( _mainProjectName, code, references );

        var service = new TransformationPreviewServiceImpl( serviceProvider );
        var result = await service.PreviewTransformationAsync( projectKey, previewedSyntaxTreeName );

        Assert.Empty( result.ErrorMessages ?? Array.Empty<string>() );
        Assert.True( result.IsSuccessful );
        Assert.NotNull( result.TransformedSyntaxTree );

        // In production, the formatting happens in the user process. For tests, we run it separately.
        var document = workspace.GetDocumentOrNull( _mainProjectName, previewedSyntaxTreeName )
            ?? workspace.GetProject( _mainProjectName ).AddDocument( previewedSyntaxTreeName, string.Empty );

        var formattedDocument = await UserProcessTransformationPreviewService.FormatOutputAsync( document, result, default );
        
        var text = await formattedDocument.GetTextAsync();

        var s = text.ToString();
        
        // Check that the output is formatted.
        Assert.DoesNotContain( "global::", s, StringComparison.Ordinal );
        
        return s;
    }

    [Fact]
    public async Task WithAspect()
    {
        var code = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;

class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}
",
            ["target.cs"] = "[MyAspect] class C {}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithInheritedAspect()
    {
        var masterCode = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;
[Inheritable]
class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}
",
            ["target.cs"] = "[MyAspect] public class C {}"
        };

        var dependentCode = new Dictionary<string, string>() { ["inherited.cs"] = "class D : C {}" };

        var result = await this.RunPreviewAsync( dependentCode, "inherited.cs", masterCode );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithProjectFabric()
    {
        var code = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}


class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender ) => amender.SelectMany( c=>c.Types ).AddAspect<MyAspect>();
} 
",
            ["target.cs"] = "class C {}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithProjectFabricAndOptions()
    {
        var code = new Dictionary<string, string>()
        {
            ["options.cs"] = OptionsTestHelper.OptionsCode,
            ["aspect.cs"] =
                """

                using Metalama.Framework.Aspects;
                using Metalama.Framework.Fabrics;
                using Metalama.Framework.Code;

                class MyAspect : TypeAspect
                {
                    [Introduce]
                    public string Field = meta.Target.Type.Enhancements().GetOptions<MyOptions>().Value;
                }


                class Fabric : ProjectFabric
                {
                    public override void AmendProject( IProjectAmender amender )
                    {
                        amender.SetOptions<MyOptions>( o => new MyOptions { Value = "TheValue" } );
                        amender.SelectMany( c=>c.Types ).AddAspect<MyAspect>();
                    }
                }

                """,
            ["target.cs"] = "class C {}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "Field = \"TheValue\"", result, StringComparison.Ordinal );
    }
    
    [Fact]
    public async Task WithTypeFabric()
    {
        var code = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;

class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}



",
            ["target.cs"] = @"

using Metalama.Framework.Fabrics;


class C {


class Fabric : TypeFabric
{
    public override void AmendType( ITypeAmender amender ) => amender.AddAspect<MyAspect>();
} 

}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithNamespaceFabric()
    {
        var code = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;

namespace Ns;

class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}



",
            ["fabric.cs"] = @"
using Metalama.Framework.Fabrics;

namespace Ns;

class Fabric : NamespaceFabric
{
    public override void AmendNamespace( INamespaceAmender amender ) => amender.SelectMany( c=>c.Types ).AddAspect<MyAspect>();
} 

",
            ["target.cs"] = @"
namespace Ns;

class C {}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithInheritedAspectChange()
    {
        using var testContext = this.CreateTestContext();
        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var masterCode1 = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;
[Inheritable]
class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod1() {}
}
",
            ["target.cs"] = "[MyAspect] public class C {}"
        };

        var dependentCode = new Dictionary<string, string>() { ["inherited.cs"] = "class D : C {}" };

        var serviceProvider = testContext.ServiceProvider.Global.WithService( pipelineFactory );

        var result1 = await RunPreviewAsync( testContext, serviceProvider, dependentCode, "inherited.cs", masterCode1 );

        Assert.Contains( "IntroducedMethod1", result1, StringComparison.Ordinal );

        var masterCode2 = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;
[Inheritable]
class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod2() {}
}
",
            ["target.cs"] = "[MyAspect] public class C {}"
        };

        var result2 = await RunPreviewAsync( testContext, serviceProvider, dependentCode, "inherited.cs", masterCode2 );

        Assert.Contains( "IntroducedMethod2", result2, StringComparison.Ordinal );
    }

    [Fact]
    public async Task WithProjectReload()
    {
        var code = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;

class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod() {}
}
",
            ["target.cs"] = "[MyAspect] class C {}"
        };

        using var testContext = this.CreateTestContext();
        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var result = await RunPreviewAsync(
            testContext,
            testContext.ServiceProvider.Global.WithService( pipelineFactory ),
            code,
            "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );

        var workspaceProvider = testContext.ServiceProvider.Global.GetRequiredService<TestWorkspaceProvider>();
        var workspace = workspaceProvider.Workspace;
        var solution = workspace.CurrentSolution;

        solution = solution.RemoveProject( workspaceProvider.GetProject( _mainProjectName ).Id );

        if ( !workspace.TryApplyChanges( solution ) )
        {
            throw new InvalidOperationException( "Removing project failed." );
        }

        result = await RunPreviewAsync(
            testContext,
            testContext.ServiceProvider.Global.WithService( pipelineFactory ),
            code,
            "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task NamespaceIntroductionWithAspect()
    {
        var code = new Dictionary<string, string>
        {
            ["aspect.cs"] = """
                using System;
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                using Metalama.Framework.Code.DeclarationBuilders;                
                
                public class MyAttribute : Attribute;

                public class TypeIntroductionsAttribute : TypeAspect
                {
                    public override void BuildAspect(IAspectBuilder<INamedType> builder)
                    {
                        var ns = builder.Advice.IntroduceNamespace(builder.Target.Compilation.GlobalNamespace, "NS").Declaration;

                        var introducedClass = builder.Advice.IntroduceClass(ns, "Introduced").Declaration;

                        builder.Advice.IntroduceField(introducedClass, "f", typeof(int));

                        builder.Advice.IntroduceAttribute(introducedClass, AttributeConstruction.Create(typeof(MyAttribute)));
                    }
                }
                """,
            ["target.cs"] = "[TypeIntroductions] class Target;"
        };

        var result = await this.RunPreviewAsync( code, "NS.Introduced.cs" );

        Assert.Contains( "namespace NS", result, StringComparison.Ordinal );
        Assert.Contains( "[My]", result, StringComparison.Ordinal );
        Assert.Contains( "class Introduced", result, StringComparison.Ordinal );
        Assert.Contains( "int f;", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task NamespaceIntroductionWithFabric()
    {
        var code = new Dictionary<string, string>
        {
            ["aspect.cs"] = """
                using System;
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                using Metalama.Framework.Code.DeclarationBuilders;                
                
                public class MyAttribute : Attribute;

                public class TypeIntroductionsAttribute : CompilationAspect
                {
                    public override void BuildAspect(IAspectBuilder<ICompilation> builder)
                    {
                        var ns = builder.Advice.IntroduceNamespace(builder.Target.GlobalNamespace, "NS").Declaration;

                        var introducedClass = builder.Advice.IntroduceClass(ns, "Introduced").Declaration;

                        builder.Advice.IntroduceField(introducedClass, "f", typeof(int));

                        builder.Advice.IntroduceAttribute(introducedClass, AttributeConstruction.Create(typeof(MyAttribute)));
                    }
                }
                """,
            ["fabric.cs"] = """
                using Metalama.Framework.Fabrics;

                class Fabric : ProjectFabric
                {
                    public override void AmendProject(IProjectAmender amender)
                    {
                        amender.AddAspect<TypeIntroductionsAttribute>();
                    }
                }
                """
        };

        var result = await this.RunPreviewAsync( code, "NS.Introduced.cs" );

        Assert.Contains( "namespace NS", result, StringComparison.Ordinal );
        Assert.Contains( "[My]", result, StringComparison.Ordinal );
        Assert.Contains( "class Introduced", result, StringComparison.Ordinal );
        Assert.Contains( "int f;", result, StringComparison.Ordinal );
    }

    [Fact]
    public async Task AssemblyAttributeIntroduction()
    {
        var code = new Dictionary<string, string>
        {
            ["aspect.cs"] = """
                using System;
                using Metalama.Framework.Advising;
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                using Metalama.Framework.Code.DeclarationBuilders;                

                public class MyAttribute : Attribute;

                public class TypeIntroductionsAttribute : CompilationAspect
                {
                    public override void BuildAspect(IAspectBuilder<ICompilation> builder)
                    {
                        builder.IntroduceAttribute(AttributeConstruction.Create(typeof(MyAttribute)));
                    }
                }
                """,
            ["fabric.cs"] = """
                using Metalama.Framework.Fabrics;

                class Fabric : ProjectFabric
                {
                    public override void AmendProject(IProjectAmender amender)
                    {
                        amender.AddAspect<TypeIntroductionsAttribute>();
                    }
                }
                """
        };

        var result = await this.RunPreviewAsync( code, "MetalamaAssemblyAttributes.cs" );

        Assert.Contains( "[assembly: My]", result, StringComparison.Ordinal );
    }
}