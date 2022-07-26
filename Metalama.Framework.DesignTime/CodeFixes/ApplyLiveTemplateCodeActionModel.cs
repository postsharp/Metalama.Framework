// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
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

    public ApplyLiveTemplateCodeActionModel( string title, string aspectTypeName, SymbolId targetSymbolId, string syntaxTreeFilePath ) : base( title )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    public override Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken )
    {
        var compilation = executionContext.Compilation.RoslynCompilation;
        var pipelineFactory = executionContext.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();

        if ( !pipelineFactory.TryGetPipeline( executionContext.ProjectId, out var pipeline ) )
        {
            return Task.FromResult( CodeActionResult.Empty );
        }

        var targetSymbol = this.TargetSymbolId.Resolve( compilation, cancellationToken: cancellationToken );

        if ( targetSymbol == null )
        {
            return Task.FromResult( CodeActionResult.Empty );
        }

        if ( pipeline.TryApplyAspectToCode(
                this.AspectTypeName,
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation,
                out _ ) )
        {
            return Task.FromResult( new CodeActionResult( outputCompilation.ModifiedSyntaxTrees.Values.Select( x => x.NewTree ) ) );
        }
        else
        {
            // How to report errors here? We will add a comment to the target symbol.
            return Task.FromResult( CodeActionResult.Empty );
        }
    }
}