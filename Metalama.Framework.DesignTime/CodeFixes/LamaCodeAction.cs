// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Metalama.Framework.DesignTime.CodeFixes;

internal sealed class LamaCodeAction : CodeAction
{
    private readonly Func<bool, CancellationToken, Task<Solution>> _createChangedSolution;

    public override string Title { get; }

    public override string? EquivalenceKey { get; }

    private LamaCodeAction( string title, Func<bool, CancellationToken, Task<Solution>> createChangedSolution, string? equivalenceKey = null )
    {
        this._createChangedSolution = createChangedSolution;
        this.Title = title;
        this.EquivalenceKey = equivalenceKey;
    }

    public static CodeAction Create( string title, Func<bool, CancellationToken, Task<Solution>> createChangedSolution, string? equivalenceKey = null )
        => new LamaCodeAction( title, createChangedSolution, equivalenceKey );

    private async Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync( bool computingPreview, CancellationToken cancellationToken )
    {
        var changedSolution = await this._createChangedSolution( computingPreview, cancellationToken ).ConfigureAwait( false );

        if ( changedSolution == null! )
        {
            return Array.Empty<CodeActionOperation>();
        }

        return new CodeActionOperation[] { new ApplyChangesOperation( changedSolution ) };
    }

    protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync( CancellationToken cancellationToken )
    {
        return this.ComputeOperationsAsync( false, cancellationToken );
    }

    protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync( CancellationToken cancellationToken )
    {
        return this.ComputeOperationsAsync( true, cancellationToken );
    }
}