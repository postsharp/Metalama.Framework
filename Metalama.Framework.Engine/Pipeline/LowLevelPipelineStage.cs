﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using System;
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

    public LowLevelPipelineStage( IAspectWeaver aspectWeaver, IBoundAspectClass aspectClass, ProjectServiceProvider serviceProvider ) : base( serviceProvider )
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
        // TODO: it is suboptimal to get a CompilationModel here.
        var compilationModel = CompilationModel.CreateInitialInstance( input.Project, input.Compilation );
        var compilation = input.Compilation.Compilation;

        var aspectInstances = input.AspectSources.Select( s => s.GetAspectInstances( compilationModel, this._aspectClass, diagnostics, cancellationToken ) )
            .SelectMany( x => x.AspectInstances )
            .GroupBy( i => i.TargetDeclaration.GetSymbol( compilation ).AssertNotNull( "The Roslyn compilation should include all introduced declarations." ) )
            .ToImmutableDictionary( g => g.Key, g => (IAspectInstance) AggregateAspectInstance.GetInstance( g ) );

        if ( !aspectInstances.Any() )
        {
            return input;
        }

        LicenseVerifier.VerifyCanUseSdk( this.ServiceProvider, this._aspectWeaver, aspectInstances.Values, diagnostics );

        var context = new AspectWeaverContext(
            this._aspectClass,
            aspectInstances,
            input.Compilation,
            diagnostics.Report,
            new AspectWeaverHelperImpl( pipelineConfiguration.ServiceProvider, compilation ),
            pipelineConfiguration.ServiceProvider.Underlying,
            input.Project,
            this._aspectClass.GeneratedCodeAnnotation,
            cancellationToken );

        var executionContext = new UserCodeExecutionContext(
            this.ServiceProvider,
            diagnostics,
            UserCodeMemberInfo.FromDelegate( new Action<AspectWeaverContext>( context1 => this._aspectWeaver.TransformAsync( context1 ) ) ) );

        var userCodeInvoker = this.ServiceProvider.GetRequiredService<UserCodeInvoker>();
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
        return new AspectPipelineResult(
            newCompilation,
            input.Project,
            input.AspectLayers,
            input.CompilationModels,
            input.Diagnostics,
            input.AspectSources );
    }
}