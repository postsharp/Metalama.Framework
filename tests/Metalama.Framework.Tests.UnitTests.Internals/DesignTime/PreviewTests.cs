// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public class PreviewTests : TestBase
{
    private async Task<string> RunPreviewAsync( Dictionary<string, string> code, string previewedSyntaxTreeName )
    {
        using var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );
        var projectKey = ProjectKey.FromCompilation( compilation );

        // Initialize the pipeline. We need to load a compilation into the pipeline, because the preview service relies on it.
        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
        var pipeline = pipelineFactory.GetOrCreatePipeline( new TestProjectOptions(), compilation ).AssertNotNull();
        await pipeline.ExecuteAsync( compilation );

        // For better test coverage, send a send compilation object (identical by content) to the pipeline, so the pipeline
        // configuration stays and the preview pipeline runs with a different compilation than the one used to initialize the pipeline.
        var compilation2 = CreateCSharpCompilation( code, name: compilation.AssemblyName );
        await pipeline.ExecuteAsync( compilation2 );

        var service = new TransformationPreviewServiceImpl( testContext.ServiceProvider.WithService( pipelineFactory ) );
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
}