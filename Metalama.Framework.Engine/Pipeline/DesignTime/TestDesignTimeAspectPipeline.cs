﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public class TestDesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
{
    public TestDesignTimeAspectPipeline( ServiceProvider serviceProvider, CompileTimeDomain? domain ) : base( serviceProvider, true, domain ) { }

    public async Task<TestDesignTimeAspectPipelineResult> ExecuteAsync( Compilation inputCompilation )
    {
        var diagnosticList = new DiagnosticBag();

        var partialCompilation = PartialCompilation.CreateComplete( inputCompilation );

        if ( !this.TryInitialize( diagnosticList, partialCompilation, null, null, CancellationToken.None, out var configuration ) )
        {
            return new TestDesignTimeAspectPipelineResult( false, diagnosticList.ToImmutableArray(), ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        // Inject a DependencyCollector so we can test exceptions based on its presence.
        configuration = configuration.WithServiceProvider( configuration.ServiceProvider.WithService( new DependencyCollector() ) );

        var stageResult = await this.ExecuteAsync( partialCompilation, diagnosticList, configuration, CancellationToken.None );

        if ( !stageResult.IsSuccessful )
        {
            return new TestDesignTimeAspectPipelineResult( false, diagnosticList.ToImmutableArray(), ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        return new TestDesignTimeAspectPipelineResult(
            true,
            stageResult.Value.Diagnostics.ReportedDiagnostics,
            stageResult.Value.AdditionalSyntaxTrees );
    }

    private class DependencyCollector : IDependencyCollector
    {
        public void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol ) { }
    }
}

public record TestDesignTimeAspectPipelineResult(
    bool Success,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<IntroducedSyntaxTree> AdditionalSyntaxTrees );