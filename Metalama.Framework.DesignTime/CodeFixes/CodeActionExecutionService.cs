// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.CodeFixes;

public class CodeActionExecutionService : ICodeActionExecutionService
{
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly ILogger _logger;

    public CodeActionExecutionService( IServiceProvider serviceProvider )
    {
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeAction" );
    }

    public async Task<CodeActionResult> ExecuteCodeActionAsync( ProjectKey projectKey, CodeActionModel codeActionModel, bool computingPreview, CancellationToken cancellationToken )
    {
        if ( !this._pipelineFactory.TryGetPipeline( projectKey, out var pipeline ) )
        {
            this._logger.Error?.Log( "Cannot get the pipeline." );

            return CodeActionResult.Error( "The Metalama code action execution service failed to get the aspect pipeline." );
        }

        var compilation = pipeline.LastCompilation;

        if ( compilation == null )
        {
            this._logger.Error?.Log( "Cannot get the compilation." );

            return CodeActionResult.Error( "The Metalama code action execution service failed to get the compilation." );
        }

        var partialCompilation = PartialCompilation.CreateComplete( compilation );

        var configuration = await pipeline.GetConfigurationAsync(
            partialCompilation,
            NullDiagnosticAdder.Instance,
            true,
            cancellationToken );

        if ( configuration == null )
        {
            this._logger.Error?.Log( "Cannot initialize the pipeline." );

            return CodeActionResult.Error( "The Metalama code action execution service failed to initialize the aspect pipeline." );
        }

        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, partialCompilation );

        var executionContext = new CodeActionExecutionContext( configuration.ServiceProvider, compilationModel, this._logger, projectKey, computingPreview );

        return await codeActionModel.ExecuteAsync( executionContext, computingPreview, cancellationToken );
    }
}