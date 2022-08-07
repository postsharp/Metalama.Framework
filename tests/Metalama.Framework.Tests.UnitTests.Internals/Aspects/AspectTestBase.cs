// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    public async Task<CompileTimeAspectPipelineResult?> CompileAsync( string code, bool throwOnError = true )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticList();

        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );

        if ( result == null && throwOnError )
        {
            throw new DiagnosticException( "The Metalama pipeline failed.", diagnostics.ToImmutableArray() );        
        }

        return result;
    }

    public async Task<CompileTimeAspectPipelineResult?> CompileAsync( IReadOnlyDictionary<string, string> code, bool throwOnError = true )
    {
        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticList();

        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );

        if ( result == null && throwOnError )
        {
            throw new DiagnosticException( "The Metalama pipeline failed.", diagnostics.ToImmutableArray() );        
        }

        return result;
    }
}