// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Testing.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.Aspects;

public class AspectTestBase : UnitTestSuite
{
    protected async Task<FallibleResult<CompileTimeAspectPipelineResult>> CompileAsync( string code, bool throwOnError = true )
    {
        using var testContext = this.CreateTestContext();
        using var domain = testContext.Domain;

        var compilation = TestCompilationFactory.CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, domain );
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
        using var testContext = this.CreateTestContext();
        using var domain = testContext.Domain;

        var compilation = TestCompilationFactory.CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, domain );
        var diagnostics = new DiagnosticBag();

        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty );

        if ( !result.IsSuccessful && throwOnError )
        {
            throw new DiagnosticException( "The Metalama pipeline failed.", diagnostics.ToImmutableArray() );
        }

        return result;
    }
}