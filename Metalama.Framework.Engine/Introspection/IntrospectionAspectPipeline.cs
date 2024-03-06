// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Introspection;

public sealed class IntrospectionAspectPipeline : AspectPipeline
{
    private readonly IIntrospectionOptionsProvider? _options;

    public IntrospectionAspectPipeline( ProjectServiceProvider serviceProvider, CompileTimeDomain domain, IIntrospectionOptionsProvider? options ) :
        base( serviceProvider, ExecutionScenario.Introspection, domain )
    {
        this._options = options;
    }

    private protected override HighLevelPipelineStage CreateHighLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
        => new LinkerPipelineStage( compileTimeProject, configuration.AspectLayers );

    private static ImmutableArray<IIntrospectionDiagnostic> MapDiagnostics( DiagnosticBag diagnostics, CompilationModel compilation )
    {
        return diagnostics
            .SelectAsImmutableArray( x => (IIntrospectionDiagnostic) new IntrospectionDiagnostic( x, compilation, DiagnosticSource.Metalama ) );
    }

    internal Task<IIntrospectionCompilationResult> ExecuteAsync( CompilationModel compilation, TestableCancellationToken cancellationToken )
    {
        var compilationName = compilation.Name ?? "(unnamed)";

        DiagnosticBag diagnostics = new();

        var introspectionFactory = new IntrospectionFactory( compilation.Compilation );

        if ( !this.TryInitialize( diagnostics, compilation.RoslynCompilation, null, null, cancellationToken, out var configuration ) )
        {
            return Task.FromResult(
                (IIntrospectionCompilationResult)
                new IntrospectionCompilationResultModel(
                    compilationName,
                    this._options,
                    false,
                    compilation,
                    MapDiagnostics( diagnostics, compilation ),
                    introspectionFactory ) );
        }

        return this.ExecuteCoreAsync( compilation, configuration, diagnostics, introspectionFactory, cancellationToken );
    }

    public Task<IIntrospectionCompilationResult> ExecuteAsync(
        PartialCompilation compilation,
        AspectPipelineConfiguration configuration,
        TestableCancellationToken cancellationToken )
    {
        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, compilation );
        var introspectionFactory = new IntrospectionFactory( compilationModel );

        return this.ExecuteCoreAsync( compilationModel, configuration, new DiagnosticBag(), introspectionFactory, cancellationToken );
    }

    private async Task<IIntrospectionCompilationResult> ExecuteCoreAsync(
        CompilationModel compilation,
        AspectPipelineConfiguration configuration,
        DiagnosticBag diagnostics,
        IntrospectionFactory introspectionFactory,
        TestableCancellationToken cancellationToken )
    {
        var compilationName = compilation.Name ?? "(unnamed)";

        var serviceProvider = configuration.ServiceProvider.Underlying.WithService( introspectionFactory );
        serviceProvider = serviceProvider.WithService( new IntrospectionPipelineListener( serviceProvider ) );

        var pipelineResult = await this.ExecuteAsync(
            compilation.PartialCompilation,
            diagnostics,
            configuration.WithServiceProvider( serviceProvider ),
            cancellationToken );

        if ( !pipelineResult.IsSuccessful )
        {
            return new IntrospectionCompilationResultModel(
                compilationName,
                this._options,
                false,
                compilation,
                MapDiagnostics( diagnostics, compilation ),
                introspectionFactory );
        }
        else
        {
            var outputCompilationModel = CompilationModel.CreateInitialInstance(
                configuration.ProjectModel,
                pipelineResult.Value.LastCompilation );

            return new IntrospectionCompilationResultModel(
                compilationName,
                this._options,
                true,
                outputCompilationModel,
                MapDiagnostics( diagnostics, compilation ),
                introspectionFactory,
                pipelineResult.Value );
        }
    }
}