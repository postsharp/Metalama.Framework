// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Testing;
using Metalama.TestFramework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.Aspects;

public class AspectTestBase : TestBase
{
    protected async Task<FallibleResult<CompileTimeAspectPipelineResult>> CompileAsync( string code, bool throwOnError = true )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticBag();

        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty );

        if ( !result.IsSuccessful && throwOnError )
        {
            throw new DiagnosticException( "The Metalama pipeline failed.", diagnostics.ToImmutableArray() );
        }

        return result;
    }

    protected async Task<FallibleResult<CompileTimeAspectPipelineResult>> CompileAsync( IReadOnlyDictionary<string, string> code, bool throwOnError = true )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticBag();

        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty );

        if ( !result.IsSuccessful && throwOnError )
        {
            throw new DiagnosticException( "The Metalama pipeline failed.", diagnostics.ToImmutableArray() );
        }

        return result;
    }
}