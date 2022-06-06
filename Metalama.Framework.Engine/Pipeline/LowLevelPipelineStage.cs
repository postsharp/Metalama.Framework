// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
/// </summary>
internal sealed class LowLevelPipelineStage : PipelineStage
{
    private readonly IAspectWeaver _aspectWeaver;
    private readonly IBoundAspectClass _aspectClass;

    public LowLevelPipelineStage( IAspectWeaver aspectWeaver, IBoundAspectClass aspectClass, IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._aspectWeaver = aspectWeaver;
        this._aspectClass = aspectClass;
    }

    /// <inheritdoc/>
    public override bool TryExecute(
        AspectPipelineConfiguration pipelineConfiguration,
        AspectPipelineResult input,
        IDiagnosticAdder diagnostics,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out AspectPipelineResult? result )
    {
        // TODO: it is suboptimal to get a CompilationModel here.
        var compilationModel = CompilationModel.CreateInitialInstance( input.Project, input.Compilation );
        var compilation = input.Compilation.Compilation;

        var aspectInstances = input.AspectSources
            .Select( s => s.GetAspectInstances( compilationModel, this._aspectClass, diagnostics, cancellationToken ) )
            .SelectMany( x => x.AspectInstances )
            .GroupBy( i => i.TargetDeclaration.GetSymbol( compilation ).AssertNotNull( "The Roslyn compilation should include all introduced declarations." ) )
            .ToImmutableDictionary( g => g.Key, g => (IAspectInstance) AggregateAspectInstance.GetInstance( g ) );

        if ( !aspectInstances.Any() )
        {
            result = input;

            return true;
        }

        var context = new AspectWeaverContext(
            this._aspectClass,
            aspectInstances,
            input.Compilation,
            diagnostics.Report,
            new AspectWeaverHelper( pipelineConfiguration.ServiceProvider, compilation ),
            pipelineConfiguration.ServiceProvider,
            input.Project,
            this._aspectClass.GeneratedCodeAnnotation );

        var executionContext = new UserCodeExecutionContext(
            this.ServiceProvider,
            diagnostics,
            UserCodeMemberInfo.FromDelegate( new Action<AspectWeaverContext>( this._aspectWeaver.Transform ) ) );

        if ( !this.ServiceProvider.GetRequiredService<UserCodeInvoker>().TryInvoke( () => this._aspectWeaver.Transform( context ), executionContext ) )
        {
            result = null;

            return false;
        }

        var newCompilation = (PartialCompilation) context.Compilation;

        // TODO: update AspectCompilation.Aspects and CompilationModels
        // (the problem here is that we don't necessarily need CompilationModels after a low-level pipeline, because
        // they are supposed to be "unmanaged" at the end of the pipeline. Currently this condition is not properly enforced,
        // and we don't test what happens when a low-level stage is before a high-level stage).
        result = new AspectPipelineResult(
            newCompilation,
            input.Project,
            input.AspectLayers,
            input.CompilationModels,
            input.Diagnostics,
            input.AspectSources );

        return true;
    }
}