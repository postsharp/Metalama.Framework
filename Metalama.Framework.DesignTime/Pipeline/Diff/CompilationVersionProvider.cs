// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Computes and caches the <see cref="CompilationChanges"/> between pairs of <see cref="Compilation"/> instances.
/// </summary>
internal class CompilationVersionProvider : IService
{
    private readonly ConditionalWeakTable<Compilation, ChangeLinkedList> _cache = new();
    private readonly SemaphoreSlim _semaphore = new( 1 );
    private readonly DiffStrategy _metalamaDiffStrategy;
    private readonly DiffStrategy _nonMetalamaDiffStrategy;
    private readonly IMetalamaProjectClassifier _metalamaProjectClassifier;

    public CompilationVersionProvider( IServiceProvider serviceProvider )
    {
        this._metalamaDiffStrategy = new DiffStrategy( serviceProvider, true, true );
        this._nonMetalamaDiffStrategy = new DiffStrategy( serviceProvider, false, true );
        this._metalamaProjectClassifier = serviceProvider.GetRequiredService<IMetalamaProjectClassifier>();
    }

    /// <summary>
    /// Computes an incremental <see cref="CompilationChanges"/> between an old compilation and a new compilation
    /// based on the values from the cache only, or returns <c>null</c> if the value was not found in the cache.
    /// </summary>
    private bool TryGetIncrementalChangesFromCache(
        Compilation oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out CompilationChanges? exactChanges,
        out CompilationChanges? closestChanges )
    {
        if ( !this._cache.TryGetValue( oldCompilation, out var list ) )
        {
            // If the old compilation is not in the cache, we cannot compute any incremental change.

            exactChanges = null;
            closestChanges = null;

            return false;
        }

        for ( var node = list.FirstIncrementalChange; node != null; node = node.Next )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( node.IncrementalChanges.NewCompilationVersion.Compilation == newCompilation )
            {
                // The exact pair old-and-new compilation was found in the cache.
                exactChanges = node.IncrementalChanges;
                closestChanges = null;

                return true;
            }
        }

        // We did not find the incremental changes for the pair of compilations, so we return the incremental changes
        // from the given original compilation to the last known compilation, which is hopefully very similar to the
        // new compilation because of the time correlation, as it should have just a few user edits.
        closestChanges = list.FirstIncrementalChange?.IncrementalChanges;
        exactChanges = null;

        return false;
    }

    public async ValueTask<CompilationVersion> GetCompilationVersionAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
    {
        // When we are asked a CompilationVersion, we do it through getting a CompilationChanges, because this path is incremental
        // and offers optimal performances.
        var changes = await this.GetCompilationChangesAsync( oldCompilation, newCompilation, cancellationToken );

        return changes.NewCompilationVersion;
    }

    private async ValueTask<ImmutableDictionary<AssemblyIdentity, ICompilationVersion>> GetReferencesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        var builder = ImmutableDictionary.CreateBuilder<AssemblyIdentity, ICompilationVersion>();
        Dictionary<AssemblyIdentity, Compilation>? oldReferences = null;

        foreach ( var reference in newCompilation.ExternalReferences.OfType<CompilationReference>() )
        {
            CompilationVersion referenceVersion;

            if ( this._cache.TryGetValue( reference.Compilation, out var list ) )
            {
                // We already have a CompilationVersion for the Compilation.
                referenceVersion = list.CompilationVersion;
            }
            else
            {
                // We don't have a CompilationVersion for the exact Compilation of the reference, but we may have a recent CompilationVersion.
                // Get the Compilation for the old value of the reference.
                Compilation? oldReferenceCompilation;

                if ( oldCompilation != null )
                {
                    oldReferences ??= oldCompilation.ExternalReferences.OfType<CompilationReference>()
                        .ToDictionary( x => x.Compilation.Assembly.Identity, x => x.Compilation );

                    oldReferences.TryGetValue( reference.Compilation.Assembly.Identity, out oldReferenceCompilation );
                }
                else
                {
                    oldReferenceCompilation = null;
                }

                referenceVersion = await this.GetCompilationVersionAsync( oldReferenceCompilation, reference.Compilation, cancellationToken );
            }

            builder.Add( reference.Compilation.Assembly.Identity, referenceVersion );
        }

        return builder.ToImmutable();
    }

    public async ValueTask<CompilationChanges> GetCompilationChangesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
    {
        DiffStrategy? diffStrategy = null;

        DiffStrategy GetDiffStrategy()
        {
            return diffStrategy ??= this._metalamaProjectClassifier.IsMetalamaEnabled( newCompilation )
                ? this._metalamaDiffStrategy
                : this._nonMetalamaDiffStrategy;
        }

        if ( oldCompilation == null )
        {
            // If we were not given an old compilation, we will return a non-incremental changes (i.e., from an empty compilation).

            if ( this._cache.TryGetValue( newCompilation, out var newList ) )
            {
                return newList.NonIncrementalChanges;
            }

            GetDiffStrategy().Observer?.OnNewCompilation();

            await this._semaphore.WaitAsync( cancellationToken );

            try
            {
                if ( !this._cache.TryGetValue( newCompilation, out newList ) )
                {
                    var references = await this.GetReferencesAsync( oldCompilation, newCompilation, cancellationToken );

                    var compilationVersion = CompilationVersion.Create( newCompilation, GetDiffStrategy(), references, cancellationToken );

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

            if ( this.TryGetIncrementalChangesFromCache(
                    oldCompilation,
                    newCompilation,
                    cancellationToken,
                    out var incrementalChanges,
                    out var closestIncrementalChanges ) )
            {
                // We already computed the changes between this exact pair of compilations.

                return incrementalChanges;
            }
            else if ( closestIncrementalChanges != null )
            {
                // We do not have the exact pair of compilations in the cache, however we have already computed a diff from the same old
                // compilation, so it's a good idea to compute the diff from the last known compilation instead of from the initial compilation,
                // as it should contain fewer changes.

                var references = await this.GetReferencesAsync(
                    closestIncrementalChanges.NewCompilationVersion.Compilation,
                    newCompilation,
                    cancellationToken );

                var changesFromClosestCompilation = CompilationChanges.Incremental(
                    closestIncrementalChanges.NewCompilationVersion,
                    newCompilation,
                    references,
                    cancellationToken );

                incrementalChanges = closestIncrementalChanges.Merge( changesFromClosestCompilation );

                if ( !this._cache.TryGetValue( oldCompilation, out var changeLinkedListFromOldCompilation ) )
                {
                    throw new AssertionFailedException();
                }

                changeLinkedListFromOldCompilation.Insert( incrementalChanges );

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

                        var oldReferences = await this.GetReferencesAsync( null, oldCompilation, cancellationToken );

                        var oldCompilationVersion = CompilationVersion.Create( oldCompilation, GetDiffStrategy(), oldReferences, cancellationToken );

                        changeLinkedListFromOldCompilation = new ChangeLinkedList( oldCompilationVersion );
                        this._cache.Add( oldCompilation, changeLinkedListFromOldCompilation );
                    }

                    var newReferences = await this.GetReferencesAsync(
                        changeLinkedListFromOldCompilation.CompilationVersion.Compilation,
                        newCompilation,
                        cancellationToken );

                    // Compute the increment.
                    incrementalChanges = CompilationChanges.Incremental(
                        changeLinkedListFromOldCompilation.CompilationVersion,
                        newCompilation,
                        newReferences,
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
        private CompilationChanges? _nonIncrementalChanges;

        public CompilationVersion CompilationVersion { get; }

        public CompilationChanges NonIncrementalChanges => this._nonIncrementalChanges ??= CompilationChanges.NonIncremental( this.CompilationVersion );

        public IncrementalChangeNode? FirstIncrementalChange { get; private set; }

        public ChangeLinkedList( CompilationVersion compilationVersion )
        {
            this.CompilationVersion = compilationVersion;
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