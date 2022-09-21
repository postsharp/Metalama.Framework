// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.CodeFixes;

internal class ApplyLiveTemplateCodeActionModel : CodeActionModel
{
    public string AspectTypeName { get; set; }

    public SymbolId TargetSymbolId { get; set; }

    public string SyntaxTreeFilePath { get; set; }

    public ApplyLiveTemplateCodeActionModel()
    {
        this.AspectTypeName = null!;
        this.SyntaxTreeFilePath = null!;
    }

    public ApplyLiveTemplateCodeActionModel(
        string title,
        string aspectTypeName,
        SymbolId targetSymbolId,
        string syntaxTreeFilePath,
        string? sourceRedistributionLicenseKey ) : base( title, aspectTypeName, sourceRedistributionLicenseKey )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    public override async Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken )
    {
        var compilation = executionContext.Compilation.RoslynCompilation;
        var pipelineFactory = executionContext.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();

        if ( !pipelineFactory.TryGetPipeline( executionContext.ProjectKey, out var pipeline ) )
        {
            return CodeActionResult.Empty;
        }

        var targetSymbol = this.TargetSymbolId.Resolve( compilation, cancellationToken: cancellationToken );

        if ( targetSymbol == null )
        {
            return CodeActionResult.Empty;
        }

        var result = await pipeline.TryApplyAspectToCode(
                this.AspectTypeName,
                compilation,
                targetSymbol,
                executionContext.ComputingPreview,
                cancellationToken ) )
        {
            return new CodeActionResult( result.Compilation!.ModifiedSyntaxTrees.Values.Select( x => x.NewTree.AssertNotNull() ) );
        }
        else
        {
            // How to report errors here? We will add a comment to the target symbol.
            return CodeActionResult.Empty;
        }
    }
}