// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;

namespace Metalama.Framework.DesignTime.CodeFixes;

public sealed class CodeActionExecutionService : ICodeActionExecutionService
{
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly ILogger _logger;
    private readonly WorkspaceProvider _workspaceProvider;

    internal CodeActionExecutionService( GlobalServiceProvider serviceProvider )
    {
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeAction" );
        this._workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();
    }

    public async Task<CodeActionResult> ExecuteCodeActionAsync(
        ProjectKey projectKey,
        CodeActionModel codeActionModel,
        bool isComputingPreview,
        CancellationToken cancellationToken )
    {
        var project = await this._workspaceProvider.GetProjectAsync( projectKey, cancellationToken );

        if ( project == null )
        {
            this._logger.Warning?.Log( "Cannot get the project." );

            return CodeActionResult.Error( "The Metalama code action execution failed because Visual Studio is not fully initialized" );
        }

        var compilation = await project.GetCompilationAsync( cancellationToken );

        if ( compilation == null )
        {
            this._logger.Warning?.Log( "Cannot get the compilation." );

            return CodeActionResult.Error( "The Metalama code action execution failed because Visual Studio is not fully initialized" );
        }

        var pipeline = this._pipelineFactory.GetOrCreatePipeline( project, cancellationToken.ToTestable() );

        if ( pipeline == null )
        {
            this._logger.Warning?.Log( "Cannot get the pipeline." );

            return CodeActionResult.Error( "The Metalama code action execution failed because Visual Studio is not fully initialized" );
        }

        var partialCompilation = PartialCompilation.CreateComplete( compilation );

        var getConfigurationResult = await pipeline.GetConfigurationAsync(
            partialCompilation,
            true,
            AsyncExecutionContext.Get(),
            cancellationToken.ToTestable() );

        if ( !getConfigurationResult.IsSuccessful )
        {
            this._logger.Warning?.Log( "Cannot initialize the pipeline." );

            return CodeActionResult.Error( "The Metalama code action execution failed because there is an error in some aspect or fabric." );
        }

        var configuration = getConfigurationResult.Value;

        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, partialCompilation );

        var executionContext = new CodeActionExecutionContext( configuration.ServiceProvider, compilationModel, this._logger, projectKey, isComputingPreview );

        return await codeActionModel.ExecuteAsync( executionContext, isComputingPreview, cancellationToken.ToTestable() );
    }
}