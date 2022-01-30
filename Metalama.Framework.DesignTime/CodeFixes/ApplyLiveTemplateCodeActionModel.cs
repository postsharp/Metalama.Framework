// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CodeFixes;

[DataContract]
public class ApplyLiveTemplateCodeActionModel : CodeActionModel
{
    [DataMember( Order = NextKey + 0 )]
    public string AspectTypeName { get; set; }

    [DataMember( Order = NextKey + 1 )]
    public string TargetSymbolId { get; set; }

    [DataMember( Order = NextKey + 2 )]
    public string SyntaxTreeFilePath { get; set; }

    public ApplyLiveTemplateCodeActionModel() { }

    public ApplyLiveTemplateCodeActionModel( string title, string aspectTypeName, string targetSymbolId, string syntaxTreeFilePath ) : base( title )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    protected override async Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken )
    {
        var compilation = executionContext.Compilation.RoslynCompilation;
        var pipelineFactory = executionContext.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();

        if ( !pipelineFactory.TryGetPipeline( executionContext.ProjectId, out var pipeline ) )
        {
            return CodeActionResult.Empty;
        }

        var targetSymbol = new SymbolId( this.TargetSymbolId ).Resolve( compilation, cancellationToken: cancellationToken );

        if ( targetSymbol == null )
        {
            return CodeActionResult.Empty;
        }

        if ( pipeline.TryApplyAspectToCode(
                this.AspectTypeName,
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation,
                out var diagnostics ) )
        {
            return new CodeActionResult( outputCompilation.ModifiedSyntaxTrees.Values.Select( x => x.NewTree ) );
        }
        else
        {
            // How to report errors here? We will add a comment to the target symbol.
            return CodeActionResult.Empty;
        }
    }
}