// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Computes and caches the <see cref="CompilationChanges"/> between pairs of <see cref="Compilation"/> instances.
/// </summary>
internal class CompilationChangesProvider
{
    private readonly ConditionalWeakTable<Compilation, ChangeLinkedList> _cache = new();
    private readonly SemaphoreSlim _semaphore = new( 1 );
    private readonly DiffStrategy _metalamaDiffStrategy;
    private readonly DiffStrategy _nonMetalamaDiffStrategy;

    public CompilationChangesProvider( IServiceProvider serviceProvider )
    {
        this._metalamaDiffStrategy = new DiffStrategy( serviceProvider, true, true );
        this._nonMetalamaDiffStrategy = new DiffStrategy( serviceProvider, false, true );
    }

    /// <summary>
    /// Computes an incremental <see cref="CompilationChanges"/> between an old compilation and a new compilation
    /// based on the values from the cache only, or returns <c>null</c> if the value was not found in the cache.
    /// </summary>
    private bool TryGetIncrementalChangesFromCache(
        Compilation oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out CompilationChanges? changes )
    {
        if ( !this._cache.TryGetValue( oldCompilation, out var list ) )
        {
            // If the old compilation is not in the cache, we cannot compute any incremental change.

            changes = null;

            return false;
        }

        for ( var node = list.FirstIncrementalChange; node != null; node = node.Next )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( node.IncrementalChanges.NewCompilationVersion.Compilation == newCompilation )
            {
                // The exact pair old-and-new compilation was found in the cache.
                changes = node.IncrementalChanges;

                return true;
            }
        }

        changes = null;

        return false;
    }

    public async ValueTask<CompilationChanges> GetCompilationChangesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        bool isMetalamaEnabled = true,
        DependencyChanges dependencyChanges = default,
        CancellationToken cancellationToken = default )
    {
        var diffStrategy = isMetalamaEnabled ? this._metalamaDiffStrategy : this._nonMetalamaDiffStrategy;

        if ( oldCompilation == null )
        {
            // If we were not given an old compilation, we will return a non-incremental changes (i.e., from an empty compilation).

            if ( this._cache.TryGetValue( newCompilation, out var newList ) )
            {
                return newList.NonIncrementalChanges;
            }

            diffStrategy.Observer?.OnNewCompilation();

            await this._semaphore.WaitAsync( cancellationToken );

            try
            {
                if ( !this._cache.TryGetValue( newCompilation, out newList ) )
                {
                    var compilationVersion = CompilationVersion.Create( newCompilation, diffStrategy, cancellationToken );

                    newList = new ChangeLinkedList( compilationVersion );
                    this._cache.Add( newCompilation, newList );
                }

                return newList.NonIncrementalChanges;
            }
            finally
            {
                this._semaphore.Release();
            }
        }
        else
        {
            // Find the pre-computed incremental changes from the graph. 

            if ( this.TryGetIncrementalChangesFromCache( oldCompilation, newCompilation, cancellationToken, out var incrementalChanges ) )
            {
                return incrementalChanges;
            }
            else
            {
                // We could not get the changes from the cache, so we need to run the diff algorithm.

                await this._semaphore.WaitAsync( cancellationToken );

                try
                {
                    if ( !this._cache.TryGetValue( oldCompilation, out var changeLinkedListFromOldCompilation ) )
                    {
                        // We have never processed the old compilation, so we have to compute it from the scratch.

                        var oldCompilationVersion = CompilationVersion.Create( oldCompilation, diffStrategy, cancellationToken );

                        changeLinkedListFromOldCompilation = new ChangeLinkedList( oldCompilationVersion );
                        this._cache.Add( oldCompilation, changeLinkedListFromOldCompilation );
                    }

                    // Compute the increment.
                    incrementalChanges = CompilationChanges.Incremental(
                        changeLinkedListFromOldCompilation.CompilationVersion,
                        newCompilation,
                        dependencyChanges,
                        cancellationToken );

                    if ( !this._cache.TryGetValue( newCompilation, out _ ) )
                    {
                        this._cache.Add( newCompilation, new ChangeLinkedList( incrementalChanges.NewCompilationVersion ) );
                    }
                    else
                    {
                        // The new compilation was already computed, but we could not reuse it because no diff from the old
                        // compilation was available.
                    }

                    changeLinkedListFromOldCompilation.Insert( incrementalChanges );

                    return incrementalChanges;
                }
                finally
                {
                    this._semaphore.Release();
                }
            }
        }
    }

    private class ChangeLinkedList
    {
        public CompilationVersion CompilationVersion { get; }

        public CompilationChanges NonIncrementalChanges { get; }

        public IncrementalChangeNode? FirstIncrementalChange { get; private set; }

        public ChangeLinkedList( CompilationVersion compilationVersion )
        {
            this.CompilationVersion = compilationVersion;
            this.NonIncrementalChanges = CompilationChanges.NonIncremental( compilationVersion );
        }

        public void Insert( CompilationChanges changes )
        {
            this.FirstIncrementalChange = new IncrementalChangeNode( changes, this.FirstIncrementalChange );
        }
    }

    private class IncrementalChangeNode
    {
        /// <summary>
        /// Gets the incremental changes between the compilation at the head of the linked list
        /// and the value of <see cref="CompilationChanges.NewCompilationVersion"/>.
        /// </summary>
        public CompilationChanges IncrementalChanges { get; }

        public IncrementalChangeNode( CompilationChanges changes, IncrementalChangeNode? next )
        {
            this.IncrementalChanges = changes;
            this.Next = next;
        }

        public IncrementalChangeNode? Next { get; }
    }
}