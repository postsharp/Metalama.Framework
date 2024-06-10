// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
/// </summary>
internal sealed class LowLevelPipelineStage : PipelineStage
{
    private readonly IAspectWeaver _aspectWeaver;
    private readonly IBoundAspectClass _aspectClass;

    public LowLevelPipelineStage( IAspectWeaver aspectWeaver, IBoundAspectClass aspectClass )
    {
        this._aspectWeaver = aspectWeaver;
        this._aspectClass = aspectClass;
    }

    /// <inheritdoc/>
    public override async Task<FallibleResult<AspectPipelineResult>> ExecuteAsync(
        AspectPipelineConfiguration pipelineConfiguration,
        AspectPipelineResult input,
        IDiagnosticAdder diagnostics,
        TestableCancellationToken cancellationToken )
    {
        var compilationModel = input.LastCompilationModel;

        var collector = new OutboundActionCollector( diagnostics );

        await Task.WhenAll(
            input.ContributorSources.AspectSources
                .Select(
                    s => s.CollectAspectInstancesAsync(
                        this._aspectClass,
                        new OutboundActionCollectionContext( collector, input.LastCompilationModel, cancellationToken ) ) ) );

        var aspectInstances = collector.AspectInstances
            .GroupBy(
                i => i.TargetDeclaration.GetSymbol( compilationModel.RoslynCompilation )
                    .AssertNotNull( "The Roslyn compilation should include all introduced declarations." ) )
            .ToImmutableDictionary( g => g.Key, g => (IAspectInstance) AggregateAspectInstance.GetInstance( g ) );

        if ( !aspectInstances.Any() )
        {
            return input;
        }

        var projectServiceProvider = pipelineConfiguration.ServiceProvider;

        var context = new AspectWeaverContext(
            this._aspectClass,
            aspectInstances,
            input.LastCompilation,
            diagnostics.Report,
            pipelineConfiguration.ServiceProvider.Underlying,
            input.Project,
            this._aspectClass.GeneratedCodeAnnotation,
            compilationModel.CompilationContext,
            compilationModel.Factory,
            compilationModel.HierarchicalOptionsManager.AssertNotNull(),
            cancellationToken );

        var executionContext = new UserCodeExecutionContext(
            projectServiceProvider,
            diagnostics,
            UserCodeDescription.Create( "calling the TransformAsync method for the weaver {0}", this._aspectWeaver.GetType() ) );

        var userCodeInvoker = projectServiceProvider.GetRequiredService<UserCodeInvoker>();
        var success = await userCodeInvoker.TryInvokeAsync( () => this._aspectWeaver.TransformAsync( context ), executionContext );

        if ( !success )
        {
            return default;
        }

        var newCompilation = (PartialCompilation) context.Compilation;

        // TODO: update AspectCompilation.Aspects and CompilationModels
        // (the problem here is that we don't necessarily need CompilationModels after a low-level pipeline, because
        // they are supposed to be "unmanaged" at the end of the pipeline. Currently this condition is not properly enforced,
        // and we don't test what happens when a low-level stage is before a high-level stage).
        var newCompilationModel = newCompilation == compilationModel.PartialCompilation ? compilationModel : null;

        return new AspectPipelineResult(
            newCompilation,
            input.Project,
            input.AspectLayers,
            input.FirstCompilationModel.AssertNotNull(),
            newCompilationModel,
            input.Configuration,
            input.Diagnostics,
            input.ContributorSources,
            aspectInstanceResults: aspectInstances.SelectAsImmutableArray(
                x => new AspectInstanceResult(
                    x.Value,
                    AdviceOutcome.Default,
                    default,
                    default,
                    default,
                    default,
                    default ) ) );
    }
}