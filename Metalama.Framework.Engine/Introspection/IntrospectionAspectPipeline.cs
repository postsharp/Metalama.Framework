// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectPipeline : AspectPipeline
{
    public IntrospectionAspectPipeline( ServiceProvider serviceProvider, CompileTimeDomain domain, bool isTest ) :
        base( serviceProvider, ExecutionScenario.Introspection, isTest, domain ) { }

    private protected override HighLevelPipelineStage CreateHighLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
        => new CompileTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    public IntrospectionCompilationResultModel Execute( CompilationModel compilation, CancellationToken cancellationToken )
    {
        DiagnosticList diagnostics = new();

        ImmutableArray<IIntrospectionDiagnostic> MapDiagnostics()
        {
            return diagnostics
                .Select( x => new IntrospectionDiagnostic( x, compilation, DiagnosticSource.Metalama ) )
                .ToImmutableArray<IIntrospectionDiagnostic>();
        }

        if ( !this.TryInitialize( diagnostics, compilation.PartialCompilation, null, cancellationToken, out var configuration ) )
        {
            return new IntrospectionCompilationResultModel( false, compilation, MapDiagnostics() );
        }

        var introspectionAspectInstanceFactory = new IntrospectionAspectInstanceFactory( compilation.Compilation );
        var serviceProvider = configuration.ServiceProvider.WithService( introspectionAspectInstanceFactory );
        serviceProvider = serviceProvider.WithService( new IntrospectionPipelineListener( serviceProvider ) );

        var success = this.TryExecute(
            compilation,
            diagnostics,
            configuration.WithServiceProvider( serviceProvider ),
            cancellationToken,
            out var pipelineResult );

        CompilationModel outputCompilationModel;

        if ( pipelineResult != null )
        {
            outputCompilationModel = CompilationModel.CreateInitialInstance(
                configuration.ProjectModel,
                pipelineResult.Compilation );
        }
        else
        {
            outputCompilationModel = compilation;
        }

        return new IntrospectionCompilationResultModel( success, outputCompilationModel, MapDiagnostics(), introspectionAspectInstanceFactory, pipelineResult );
    }
}