// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Testing;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public class PreviewTests : TestBase
{
    private Task<string> RunPreviewAsync(
        Dictionary<string, string> code,
        string previewedSyntaxTreeName,
        Dictionary<string, string>? dependencyCode = null )
    {
        using var testContext = this.CreateTestContext();
        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        return RunPreviewAsync( pipelineFactory, testContext.ServiceProvider, code, previewedSyntaxTreeName, dependencyCode );
    }

    private static async Task<string> RunPreviewAsync(
        TestDesignTimeAspectPipelineFactory pipelineFactory,
        ProjectServiceProvider serviceProvider,
        Dictionary<string, string> code,
        string previewedSyntaxTreeName,
        Dictionary<string, string>? dependencyCode = null )
    {
        MetadataReference[]? references;

        if ( dependencyCode == null )
        {
            references = null;
        }
        else
        {
            var dependencyCompilation = CreateCSharpCompilation( dependencyCode, name: "dependency" );
            references = new MetadataReference[] { dependencyCompilation.ToMetadataReference() };
        }

        var compilation = CreateCSharpCompilation( code, additionalReferences: references, name: "main" );
        var projectKey = ProjectKeyFactory.FromCompilation( compilation );

        // Initialize the pipeline. We need to load a compilation into the pipeline, because the preview service relies on it.

        var pipeline = pipelineFactory.GetOrCreatePipeline( new TestProjectOptions(), compilation ).AssertNotNull();
        Assert.True( (await pipeline.ExecuteAsync( compilation )).IsSuccessful );

        // For better test coverage, send a send compilation object (identical by content) to the pipeline, so the pipeline
        // configuration stays and the preview pipeline runs with a different compilation than the one used to initialize the pipeline.
        var compilation2 = CreateCSharpCompilation( code, name: compilation.AssemblyName, additionalReferences: references );
        Assert.True( (await pipeline.ExecuteAsync( compilation2 )).IsSuccessful );

        var service = new TransformationPreviewServiceImpl( serviceProvider.WithService( pipelineFactory ) );
        var result = await service.PreviewTransformationAsync( projectKey, previewedSyntaxTreeName );

        Assert.True( result.IsSuccessful );
        Assert.NotNull( result.TransformedSourceText );

        return result.TransformedSourceText!;
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
[Inherited]
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
    public override void AmendProject( IProjectAmender amender ) => amender.With( c=>c.Types ).AddAspect<MyAspect>();
} 
",
            ["target.cs"] = "class C {}"
        };

        var result = await this.RunPreviewAsync( code, "target.cs" );

        Assert.Contains( "IntroducedMethod", result, StringComparison.Ordinal );
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
    public override void AmendType( ITypeAmender amender ) => amender.With( c=>c ).AddAspect<MyAspect>();
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
    public override void AmendNamespace( INamespaceAmender amender ) => amender.With( c=>c.Types ).AddAspect<MyAspect>();
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
[Inherited]
class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod1() {}
}
",
            ["target.cs"] = "[MyAspect] public class C {}"
        };

        var dependentCode = new Dictionary<string, string>() { ["inherited.cs"] = "class D : C {}" };

        var result1 = await RunPreviewAsync( pipelineFactory, testContext.ServiceProvider, dependentCode, "inherited.cs", masterCode1 );

        Assert.Contains( "IntroducedMethod1", result1, StringComparison.Ordinal );

        var masterCode2 = new Dictionary<string, string>()
        {
            ["aspect.cs"] = @"
using Metalama.Framework.Aspects;
[Inherited]
class MyAspect : TypeAspect
{
   [Introduce]
   void IntroducedMethod2() {}
}
",
            ["target.cs"] = "[MyAspect] public class C {}"
        };

        var result2 = await RunPreviewAsync( pipelineFactory, testContext.ServiceProvider, dependentCode, "inherited.cs", masterCode2 );

        Assert.Contains( "IntroducedMethod2", result2, StringComparison.Ordinal );
    }
}