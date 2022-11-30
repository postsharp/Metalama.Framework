// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Represents a code action specified by the user in an aspect.
/// </summary>
internal class UserCodeActionModel : CodeActionModel
{
    public UserCodeActionModel()
    {
        this.DiagnosticId = null!;
        this.SyntaxTreeFilePath = null!;
    }

    public UserCodeActionModel(
        string title,
        Diagnostic diagnostic ) : base( title )
    {
        this.DiagnosticId = diagnostic.Id;
        this.DiagnosticSpan = diagnostic.Location.SourceSpan;
        this.SyntaxTreeFilePath = diagnostic.Location.SourceTree!.FilePath;
    }

    public string DiagnosticId { get; init; }

    public TextSpan DiagnosticSpan { get; init; }

    public string SyntaxTreeFilePath { get; init; }

    public override async Task<CodeActionResult> ExecuteAsync(
        CodeActionExecutionContext executionContext,
        bool isComputingPreview,
        TestableCancellationToken cancellationToken )
    {
        var pipelineFactory = executionContext.ServiceProvider.Global.GetRequiredService<DesignTimeAspectPipelineFactory>();

        if ( !pipelineFactory.TryGetPipeline( executionContext.ProjectKey, out var pipeline ) )
        {
            executionContext.Logger.Warning?.Log( "Could not get the pipeline." );

            return CodeActionResult.Empty;
        }

        var compilation = pipeline.LastCompilation;

        if ( compilation == null )
        {
            executionContext.Logger.Warning?.Log( "Could not get the compilation." );

            return CodeActionResult.Empty;
        }

        var syntaxTree = compilation.SyntaxTrees.FirstOrDefault( x => x.FilePath == this.SyntaxTreeFilePath );

        if ( syntaxTree == null )
        {
            executionContext.Logger.Warning?.Log( "Could not get the syntax tree." );

            return CodeActionResult.Empty;
        }

        var codeFixRunner = new DesignTimeCodeFixRunner( executionContext.ServiceProvider );

        return await codeFixRunner.ExecuteCodeFixAsync(
            compilation,
            syntaxTree,
            this.DiagnosticId,
            this.DiagnosticSpan,
            this.Title,
            isComputingPreview,
            cancellationToken );
    }
}