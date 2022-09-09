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

    public async Task<CodeActionResult> ExecuteCodeActionAsync( string projectId, CodeActionModel codeActionModel, bool computingPreview, CancellationToken cancellationToken )
    {
        if ( !this._pipelineFactory.TryGetPipeline( projectId, out var pipeline ) )
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

        if ( !pipeline.TryGetConfiguration(
                partialCompilation,
                NullDiagnosticAdder.Instance,
                true,
                cancellationToken,
                out var configuration ) )
        {
            this._logger.Error?.Log( "Cannot initialize the pipeline." );

            return CodeActionResult.Empty;
        }

        if ( !computingPreview )
        {
            // This place can handle all design-time restrictions, which are code-fixes and live templates. (Which are code-fixes as well.)
            // TODO:
            // Should we check here or in TryGetConfiguration?
            // Do we want to use the license from MSBuild?
            // Do we want to keep the change of ExecutionScenario?
            throw new NotImplementedException( "Check license." );
        }

        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, partialCompilation );

        var executionContext = new CodeActionExecutionContext( configuration.ServiceProvider, compilationModel, this._logger, projectId, computingPreview );

        return await codeActionModel.ExecuteAsync( executionContext, cancellationToken );
    }
}