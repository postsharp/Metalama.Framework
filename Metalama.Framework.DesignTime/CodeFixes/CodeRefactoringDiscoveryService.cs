// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Configuration;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Implementation of <see cref="ICodeRefactoringDiscoveryService"/>, which runs in the analysis process.
/// </summary>
public class CodeRefactoringDiscoveryService : ICodeRefactoringDiscoveryService
{
    private readonly ILogger _logger;
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly DesignTimeConfiguration _licensingConfiguration;
    private readonly WorkspaceProvider _workspaceProvider;

    public CodeRefactoringDiscoveryService( GlobalServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeRefactoring" );
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._licensingConfiguration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<DesignTimeConfiguration>();
        this._workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();
    }

    public async Task<ComputeRefactoringResult> ComputeRefactoringsAsync(
        ProjectKey projectKey,
        string syntaxTreePath,
        TextSpan span,
        CancellationToken cancellationToken )
    {
        if ( !this._pipelineFactory.TryGetPipeline( projectKey, out var pipeline ) )
        {
            this._logger.Warning?.Log( $"ComputeRefactorings('{projectKey}', '{syntaxTreePath}'): cannot get the pipeline for project '{projectKey}'." );

            return ComputeRefactoringResult.Empty;
        }

        var compilation = await this._workspaceProvider.GetCompilationAsync( projectKey, cancellationToken );

        if ( compilation == null )
        {
            this._logger.Warning?.Log( $"ComputeRefactorings('{projectKey}', '{syntaxTreePath}'): cannot get the compilation for project '{projectKey}'." );

            return ComputeRefactoringResult.Empty;
        }

        var syntaxTree = compilation.SyntaxTrees.FirstOrDefault( x => x.FilePath == syntaxTreePath );

        if ( syntaxTree == null )
        {
            this._logger.Warning?.Log(
                $"ComputeRefactorings('{projectKey}', '{syntaxTreePath}'): cannot get the SyntaxTree '{syntaxTreePath}' in project '{projectKey}'." );

            return ComputeRefactoringResult.Empty;
        }

        var node = (await syntaxTree.GetRootAsync( cancellationToken )).FindNode( span );

        var semanticModel = compilation.GetCachedSemanticModel( syntaxTree );

        var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

        if ( symbol == null )
        {
            return ComputeRefactoringResult.Empty;
        }

        // Execute the pipeline.

        var eligibleAspects = pipeline.GetEligibleAspects( compilation, symbol, cancellationToken.ToTestable() );

        var aspectActions = new CodeActionMenuModel( "Add aspect" );
        var liveTemplatesActions = new CodeActionMenuModel( "Apply live template" );

        var licenseVerifier = pipeline.ServiceProvider.GetService<LicenseVerifier>();

        foreach ( var aspect in eligibleAspects )
        {
            var targetSymbolId = SymbolId.Create( symbol );

            aspectActions.Items.Add(
                new AddAspectAttributeCodeActionModel(
                    aspect.FullName,
                    targetSymbolId,
                    syntaxTreePath ) );

            if ( aspect.IsLiveTemplate && (!this._licensingConfiguration.HideUnlicensedCodeActions || licenseVerifier == null
                                                                                                   || licenseVerifier.VerifyCanApplyCodeFix( aspect )) )
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