// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.LiveTemplates;

/// <summary>
/// An implementation of the <see cref="AspectPipeline"/> that applies an aspect to source code in the interactive process.
/// </summary>
public class LiveTemplateAspectPipeline : AspectPipeline
{
    private readonly Func<AspectPipelineConfiguration, IAspectClass> _aspectSelector;
    private readonly ISymbol _targetSymbol;

    private LiveTemplateAspectPipeline(
        ServiceProvider serviceProvider,
        CompileTimeDomain domain,
        Func<AspectPipelineConfiguration, IAspectClass> aspectSelector,
        ISymbol targetSymbol ) : base( serviceProvider, ExecutionScenario.LiveTemplate, false, domain )
    {
        this._aspectSelector = aspectSelector;
        this._targetSymbol = targetSymbol;
    }

    private protected override (ImmutableArray<IAspectSource> AspectSources, ImmutableArray<IValidatorSource> ValidatorSources) CreateAspectSources(
        AspectPipelineConfiguration configuration,
        Compilation compilation,
        CancellationToken cancellationToken )
    {
        var aspectClass = this._aspectSelector( configuration );

        return (ImmutableArray.Create<IAspectSource>( new AspectSource( this, aspectClass ) ), ImmutableArray<IValidatorSource>.Empty);
    }

    public static async Task<FallibleResult<PartialCompilation>> ExecuteAsync(
        ServiceProvider serviceProvider,
        CompileTimeDomain domain,
        AspectPipelineConfiguration? pipelineConfiguration,
        Func<AspectPipelineConfiguration, IAspectClass> aspectSelector,
        PartialCompilation inputCompilation,
        ISymbol targetSymbol,
        IDiagnosticAdder diagnosticAdder,
        bool isComputingPreview,
        CancellationToken cancellationToken )
    {
        LiveTemplateAspectPipeline pipeline = new( serviceProvider, domain, aspectSelector, targetSymbol );

        var result = await pipeline.ExecuteAsync( inputCompilation, diagnosticAdder, pipelineConfiguration, cancellationToken );

        // Enforce licensing
        var aspectInstance = result.Value.AspectInstanceResults.Single().AspectInstance;
        var aspectClass = aspectInstance.AspectClass;

        if ( !isComputingPreview && !LicenseVerifier.VerifyCanApplyLiveTemplate( serviceProvider, aspectClass, diagnosticAdder ) )
        {
            diagnosticAdder.Report(
                LicensingDiagnosticDescriptors.CodeActionNotAvailable.CreateRoslynDiagnostic(
                    aspectInstance.TargetDeclaration.GetSymbol( result.Value.Compilation.Compilation )
                        .AssertNotNull( "Live templates should be always applied on a target." )
                        .GetDiagnosticLocation(),
                    ($"Apply [{aspectClass.DisplayName}] aspect", aspectClass.DisplayName) ) );

            return default;
        }

        if ( !result.IsSuccessful )
        {
            return default;
        }
        else
        {
            return result.Value.Compilation;
        }
    }

    private protected override HighLevelPipelineStage CreateHighLevelStage(
        PipelineStageConfiguration configuration,
        CompileTimeProject compileTimeProject )
        => new CompileTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    private class AspectSource : IAspectSource
    {
        private readonly LiveTemplateAspectPipeline _parent;

        public AspectSource( LiveTemplateAspectPipeline parent, IAspectClass aspectClass )
        {
            this._parent = parent;

            this.AspectClasses = ImmutableArray.Create( aspectClass );
        }

        public ImmutableArray<IAspectClass> AspectClasses { get; }

        public AspectSourceResult GetAspectInstances(
            CompilationModel compilation,
            IAspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
        {
            var targetDeclaration = compilation.Factory.GetDeclaration( this._parent._targetSymbol );

            return new AspectSourceResult(
                new[]
                {
                    ((AspectClass) aspectClass).CreateAspectInstance(
                        targetDeclaration,
                        (IAspect) Activator.CreateInstance( this.AspectClasses[0].Type ).AssertNotNull(),
                        default )
                } );
        }
    }
}