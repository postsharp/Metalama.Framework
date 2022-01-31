// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

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

    public async Task<CodeActionResult> ExecuteCodeActionAsync( string projectId, CodeActionModel codeActionModel, CancellationToken cancellationToken )
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

        var compilationModel = CompilationModel.CreateInitialInstance( configuration.ProjectModel, partialCompilation );

        var executionContext = new CodeActionExecutionContext( configuration.ServiceProvider, compilationModel, this._logger, projectId );

        return await codeActionModel.ExecuteAsync( executionContext, cancellationToken );
    }
}

public class CodeActionDiscoveryService : ICodeActionDiscoveryService
{
    private readonly ILogger _logger;
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

    public CodeActionDiscoveryService( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeRefactoring" );
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
    }

    public async Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
        string projectId,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        if ( !this._pipelineFactory.TryGetPipeline( projectId, out var pipeline ) )
        {
            this._logger.Warning?.Log( $"ComputeRefactorings('{projectId}', '{syntaxTreePath}'): cannot get the pipeline for project '{projectId}'." );

            return ComputeRefactoringResult.Empty;
        }

        var compilation = pipeline.LastCompilation;

        if ( compilation == null )
        {
            this._logger.Warning?.Log( $"ComputeRefactorings('{projectId}', '{syntaxTreePath}'): cannot get the compilation for project '{projectId}'." );

            return ComputeRefactoringResult.Empty;
        }

        var syntaxTree = compilation.SyntaxTrees.FirstOrDefault( x => x.FilePath == syntaxTreePath );

        if ( syntaxTree == null )
        {
            this._logger.Warning?.Log(
                $"ComputeRefactorings('{projectId}', '{syntaxTreePath}'): cannot get the SyntaxTree '{syntaxTreePath}' in project '{projectId}'." );

            return ComputeRefactoringResult.Empty;
        }

        var node = (await syntaxTree.GetRootAsync( cancellationToken )).FindNode( span );

        var semanticModel = compilation.GetSemanticModel( syntaxTree );

        var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

        if ( symbol == null )
        {
            return ComputeRefactoringResult.Empty;
        }

        // Execute the pipeline.

        var eligibleAspects = pipeline.GetEligibleAspects( compilation, symbol, cancellationToken );

        var aspectActions = new CodeActionMenuModel( "Add aspect" );
        var liveTemplatesActions = new CodeActionMenuModel( "Apply live template" );

        foreach ( var aspect in eligibleAspects )
        {
            var targetSymbolId = SymbolId.Create( symbol ).ToString();
            aspectActions.Items.Add( new AddAspectAttributeCodeActionModel( aspect.FullName, targetSymbolId, syntaxTreePath ) );

            if ( aspect.IsLiveTemplate )
            {
                liveTemplatesActions.Items.Add(
                    new ApplyLiveTemplateCodeActionModel(
                        aspect.DisplayName,
                        aspect.FullName,
                        targetSymbolId,
                        syntaxTreePath ) );
            }
        }

        var topLevelActions = ImmutableArray.CreateBuilder<CodeActionBaseModel>();

        if ( aspectActions.Items.Count > 0 )
        {
            topLevelActions.Add( aspectActions );
        }

        if ( liveTemplatesActions.Items.Count > 0 )
        {
            topLevelActions.Add( liveTemplatesActions );
        }

        return new ComputeRefactoringResult( topLevelActions.ToImmutable() );
    }
}