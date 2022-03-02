// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public class TestDesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
{
    public TestDesignTimeAspectPipeline( ServiceProvider serviceProvider, CompileTimeDomain? domain ) : base( serviceProvider, true, domain ) { }

    public TestDesignTimeAspectPipelineResult Execute( Compilation inputCompilation )
    {
        var diagnosticList = new DiagnosticList();

        var partialCompilation = PartialCompilation.CreateComplete( inputCompilation );

        if ( !this.TryInitialize( diagnosticList, partialCompilation, null, CancellationToken.None, out var configuration ) )
        {
            return new TestDesignTimeAspectPipelineResult( false, diagnosticList.ToImmutableArray(), ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        if ( !this.TryExecute( partialCompilation, diagnosticList, configuration, CancellationToken.None, out var stageResult ) )
        {
            return new TestDesignTimeAspectPipelineResult( false, diagnosticList.ToImmutableArray(), ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        return new TestDesignTimeAspectPipelineResult(
            true,
            stageResult.Diagnostics.ReportedDiagnostics,
            stageResult.AdditionalSyntaxTrees );
    }
}

public record TestDesignTimeAspectPipelineResult(
    bool Success,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<IntroducedSyntaxTree> AdditionalSyntaxTrees );