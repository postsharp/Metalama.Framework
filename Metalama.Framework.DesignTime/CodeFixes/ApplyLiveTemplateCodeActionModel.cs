// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.SerializableIds;
using Metalama.Framework.Engine.Utilities.Threading;
using Newtonsoft.Json;

namespace Metalama.Framework.DesignTime.CodeFixes;

[JsonObject]
internal sealed class ApplyLiveTemplateCodeActionModel : CodeActionModel
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
        string aspectTypeName,
        SymbolId targetSymbolId,
        string syntaxTreeFilePath,
        string title ) : base( title )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    public override async Task<CodeActionResult> ExecuteAsync(
        CodeActionExecutionContext executionContext,
        bool isComputingPreview,
        TestableCancellationToken cancellationToken )
    {
        var compilation = executionContext.Compilation.RoslynCompilation;
        var pipelineFactory = executionContext.ServiceProvider.Global.GetRequiredService<DesignTimeAspectPipelineFactory>();

        if ( !pipelineFactory.TryGetPipeline( executionContext.ProjectKey, out var pipeline ) )
        {
            return CodeActionResult.Empty;
        }

        var targetSymbol = this.TargetSymbolId.Resolve( compilation, cancellationToken: cancellationToken );

        if ( targetSymbol == null )
        {
            return CodeActionResult.Empty;
        }

        var result = await pipeline.ApplyAspectToCodeAsync(
            this.AspectTypeName,
            compilation,
            targetSymbol,
            executionContext.IsComputingPreview,
            cancellationToken );

        if ( result.Success )
        {
            return CodeActionResult.Success( result.Compilation!.ModifiedSyntaxTrees.Values.Select( x => x.NewTree.AssertNotNull() ) );
        }
        else
        {
            return CodeActionResult.Error( result.Diagnostics );
        }
    }
}