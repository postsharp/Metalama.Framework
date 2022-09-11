// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

public class CodeActionResult
{
    public ImmutableArray<SerializableSyntaxTree> SyntaxTreeChanges { get; }

    [JsonConstructor]
    public CodeActionResult( ImmutableArray<SerializableSyntaxTree> syntaxTreeChanges )
    {
        this.SyntaxTreeChanges = syntaxTreeChanges;
    }

    public CodeActionResult( IEnumerable<SyntaxTree> modifiedTrees ) :
        this( modifiedTrees.Select( x => new SerializableSyntaxTree( x ) ).ToImmutableArray() ) { }

    public static CodeActionResult Empty { get; } = new( ImmutableArray<SerializableSyntaxTree>.Empty );

    public async ValueTask<Solution> ApplyAsync( Microsoft.CodeAnalysis.Project project, ILogger logger, bool format, CancellationToken cancellationToken )
    {
        var solution = project.Solution;

        // Apply changes.
        foreach ( var change in this.SyntaxTreeChanges )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = project.Documents.FirstOrDefault( x => x.FilePath == change.FilePath );

            if ( document == null )
            {
                logger?.Warning?.Log( $"Cannot map changes to solution: Cannot find document '{change.FilePath}'." );

                continue;
            }

            solution = solution.WithDocumentSyntaxRoot( document.Id, change.GetAnnotatedSyntaxNode( cancellationToken ) );

            if ( format )
            {
                var formatted = (await OutputCodeFormatter.FormatToDocumentAsync(
                    solution.GetDocument( document.Id )!,
                    null,
                    false,
                    cancellationToken )).Syntax;

                solution = solution.WithDocumentSyntaxRoot( document.Id, formatted );
            }
        }

        return solution;
    }
}