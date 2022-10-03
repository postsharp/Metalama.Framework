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

    public async Task<CodeActionResult> ExecuteCodeActionAsync( ProjectKey projectKey, CodeActionModel codeActionModel, CancellationToken cancellationToken )
    {
        if ( !this._pipelineFactory.TryGetPipeline( projectKey, out var pipeline ) )
        {
            this._logger.Error?.Log( "Cannot get the pipeline." );

            return CodeActionResult.Empty;
        }

        var compilation = pipeline.LastCompilation;

        if ( compilation == null )
        {
            this._logger.Error?.Log( "Cannot get the compilation." );

            return CodeActionResult.Empty;
        }

        var partialCompilation = PartialCompilation.CreateComplete( compilation );

        var getConfigurationResult = await pipeline.GetConfigurationAsync(
            partialCompilation,
            true,
            cancellationToken );

        if ( !getConfigurationResult.IsSuccess )
        {
            this._logger.Error?.Log( "Cannot initialize the pipeline." );

            return CodeActionResult.Empty;
        }

        var configuration = getConfigurationResult.Value;

        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, partialCompilation );

        var executionContext = new CodeActionExecutionContext( configuration.ServiceProvider, compilationModel, this._logger, projectKey );

        return await codeActionModel.ExecuteAsync( executionContext, cancellationToken );
    }
}