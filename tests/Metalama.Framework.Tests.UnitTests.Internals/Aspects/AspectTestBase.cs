// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.TestFramework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.Aspects;

public class AspectTestBase : TestBase
{
    public async Task<CompileTimeAspectPipelineResult?> CompileAsync( string code )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticList();

        return await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );
    }

    public async Task<CompileTimeAspectPipelineResult?> CompileAsync( IReadOnlyDictionary<string, string> code )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticList();

        return await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );
    }
}