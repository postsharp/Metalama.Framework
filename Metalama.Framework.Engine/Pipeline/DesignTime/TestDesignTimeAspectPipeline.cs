// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public sealed class TestDesignTimeAspectPipeline : BaseDesignTimeAspectPipeline
{
    public TestDesignTimeAspectPipeline( in ProjectServiceProvider serviceProvider, CompileTimeDomain? domain ) : base( serviceProvider, domain ) { }

    public async Task<TestDesignTimeAspectPipelineResult> ExecuteAsync( Compilation inputCompilation )
    {
        var diagnosticList = new DiagnosticBag();

        var partialCompilation = PartialCompilation.CreateComplete( inputCompilation );

        if ( !this.TryInitialize( diagnosticList, partialCompilation.Compilation, null, null, CancellationToken.None, out var configuration ) )
        {
            return new TestDesignTimeAspectPipelineResult(
                false,
                diagnosticList.ToImmutableArray(),
                ImmutableArray<ScopedSuppression>.Empty,
                ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        // Inject a DependencyCollector so we can test exceptions based on its presence.
        configuration = configuration.WithServiceProvider( configuration.ServiceProvider.Underlying.WithService( new DependencyCollector() ) );

        var stageResult = await this.ExecuteAsync( partialCompilation, diagnosticList, configuration, TestableCancellationToken.None );

        if ( !stageResult.IsSuccessful )
        {
            return new TestDesignTimeAspectPipelineResult(
                false,
                diagnosticList.ToImmutableArray(),
                ImmutableArray<ScopedSuppression>.Empty,
                ImmutableArray<IntroducedSyntaxTree>.Empty );
        }

        return new TestDesignTimeAspectPipelineResult(
            true,
            stageResult.Value.Diagnostics.ReportedDiagnostics,
            stageResult.Value.Diagnostics.DiagnosticSuppressions,
            stageResult.Value.AdditionalSyntaxTrees );
    }

    private sealed class DependencyCollector : IDependencyCollector
    {
        public void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol ) { }
    }
}