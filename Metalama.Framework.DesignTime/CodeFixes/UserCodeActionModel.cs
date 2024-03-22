// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Represents a code action specified by the user in an aspect.
/// </summary>
[JsonObject]
internal sealed class UserCodeActionModel : CodeActionModel
{
    public string CompleteTitle { get; init; }

    public UserCodeActionModel()
    {
        this.DiagnosticId = null!;
        this.SyntaxTreeFilePath = null!;
        this.CompleteTitle = null!;
    }

    public UserCodeActionModel(
        string title,
        string completeTitle,
        Diagnostic diagnostic ) : base( title )
    {
        this.DiagnosticId = diagnostic.Id;
        this.DiagnosticSpan = diagnostic.Location.SourceSpan;
        this.SyntaxTreeFilePath = diagnostic.Location.SourceTree!.FilePath;
        this.CompleteTitle = completeTitle;
    }

    public string DiagnosticId { get; init; }

    public TextSpan DiagnosticSpan { get; init; }

    public string SyntaxTreeFilePath { get; init; }

    public override async Task<CodeActionResult> ExecuteAsync(
        CodeActionExecutionContext executionContext,
        bool isComputingPreview,
        TestableCancellationToken cancellationToken )
    {
        if ( !executionContext.Compilation.PartialCompilation.SyntaxTrees.TryGetValue( this.SyntaxTreeFilePath, out var syntaxTree ) )
        {
            executionContext.Logger.Warning?.Log( "Could not get the syntax tree." );

            return CodeActionResult.Empty;
        }

        var codeFixRunner = new DesignTimeCodeFixRunner( executionContext.ServiceProvider );

        return await codeFixRunner.ExecuteCodeFixAsync(
            executionContext.Compilation.RoslynCompilation,
            syntaxTree,
            this.DiagnosticId,
            this.DiagnosticSpan,
            this.CompleteTitle,
            isComputingPreview,
            cancellationToken );
    }
}