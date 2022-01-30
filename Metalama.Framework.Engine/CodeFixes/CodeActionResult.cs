// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

[DataContract]
public class CodeActionResult
{
    [DataMember( Order = 0 )]
    public ImmutableArray<SyntaxTreeChange> SyntaxTreeChanges { get; }

    public CodeActionResult( ImmutableArray<SyntaxTreeChange> syntaxTreeChanges )
    {
        this.SyntaxTreeChanges = syntaxTreeChanges;
    }

    public CodeActionResult( IEnumerable<SyntaxTree> modifiedTrees ) : this( modifiedTrees.Select( x => new SyntaxTreeChange( x ) ).ToImmutableArray() ) { }

    public static CodeActionResult Empty { get; } = new( ImmutableArray<SyntaxTreeChange>.Empty );

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

            if ( format )
            {
                document = (await OutputCodeFormatter.FormatToDocumentAsync(
                    document,
                    null,
                    false,
                    cancellationToken )).Document;
            }

            solution = solution.WithDocumentSyntaxRoot( document.Id, change.Root.Node );
        }

        return solution;
    }
}