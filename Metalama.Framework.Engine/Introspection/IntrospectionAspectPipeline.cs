// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectPipeline : AspectPipeline
{
    public IntrospectionAspectPipeline( ServiceProvider serviceProvider, CompileTimeDomain domain, bool isTest ) :
        base( serviceProvider, ExecutionScenario.Introspection, isTest, domain ) { }

    private protected override HighLevelPipelineStage CreateHighLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
        => new CompileTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    public async Task<IntrospectionCompilationResultModel> ExecuteAsync( CompilationModel compilation, TestableCancellationToken cancellationToken )
    {
        DiagnosticBag diagnostics = new();

        ImmutableArray<IIntrospectionDiagnostic> MapDiagnostics()
        {
            return diagnostics
                .Select( x => new IntrospectionDiagnostic( x, compilation, DiagnosticSource.Metalama ) )
                .ToImmutableArray<IIntrospectionDiagnostic>();
        }

        if ( !this.TryInitialize( diagnostics, compilation.PartialCompilation, null, null, cancellationToken, out var configuration ) )
        {
            return new IntrospectionCompilationResultModel( false, compilation, MapDiagnostics() );
        }

        var introspectionAspectInstanceFactory = new IntrospectionAspectInstanceFactory( compilation.Compilation );
        var serviceProvider = configuration.ServiceProvider.WithService( introspectionAspectInstanceFactory );
        serviceProvider = serviceProvider.WithService( new IntrospectionPipelineListener( serviceProvider ) );

        var pipelineResult = await this.ExecuteAsync(
            compilation,
            diagnostics,
            configuration.WithServiceProvider( serviceProvider ),
            cancellationToken );

        if ( !pipelineResult.IsSuccessful )
        {
            return new IntrospectionCompilationResultModel( false, compilation, MapDiagnostics(), introspectionAspectInstanceFactory );
        }
        else
        {
            var outputCompilationModel = CompilationModel.CreateInitialInstance(
                configuration.ProjectModel,
                pipelineResult.Value.Compilation );

            return new IntrospectionCompilationResultModel(
                true,
                outputCompilationModel,
                MapDiagnostics(),
                introspectionAspectInstanceFactory,
                pipelineResult.Value );
        }
    }
}