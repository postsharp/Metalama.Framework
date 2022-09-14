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
    private readonly Implementation _implementation;

    public CompilationVersionProvider( IServiceProvider serviceProvider )
    {
        this._implementation = new Implementation( serviceProvider );
    }

    public ValueTask<CompilationVersion> GetCompilationVersionAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
        => this._implementation.GetCompilationVersionCoreAsync( oldCompilation, newCompilation, false, cancellationToken );

    public ValueTask<CompilationChanges> GetCompilationChangesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
        => this._implementation.GetCompilationChangesAsyncCore( oldCompilation, newCompilation, false, cancellationToken );

    public ValueTask<CompilationChanges> MergeChangesAsync( CompilationChanges first, CompilationChanges second, CancellationToken cancellationToken )
        => this._implementation.MergeChangesCoreAsync( first, second, false, cancellationToken );

    public ValueTask<ReferencedCompilationChange> MergeChangesAsync(
        ReferencedCompilationChange first,
        ReferencedCompilationChange second,
        CancellationToken cancellationToken = default )
        => this._implementation.MergeChangesCoreAsync( first, second, false, cancellationToken );

    private class Implementation
    {
        private readonly ConditionalWeakTable<Compilation, ChangeLinkedList> _cache = new();
        private readonly SemaphoreSlim _semaphore = new( 1 );
        private readonly DiffStrategy _metalamaDiffStrategy;
        private readonly DiffStrategy _nonMetalamaDiffStrategy;
        private readonly IMetalamaProjectClassifier _metalamaProjectClassifier;

        public Implementation( IServiceProvider serviceProvider )
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

        public async ValueTask<CompilationVersion> GetCompilationVersionCoreAsync(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
            CancellationToken cancellationToken = default )
        {
            // When we are asked a CompilationVersion, we do it through getting a CompilationChanges, because this path is incremental
            // and offers optimal performances.
            var changes = await this.GetCompilationChangesAsyncCore( oldCompilation, newCompilation, semaphoreOwned, cancellationToken );

            return changes.NewCompilationVersion;
        }

        private async ValueTask<(ImmutableDictionary<AssemblyIdentity, ICompilationVersion> References,
            ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange> Changes)> GetReferencesAsync(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
            CancellationToken cancellationToken )
        {
            // If references are the same by reference, there is no need to compare anything. Most hits to the method should take this shortcut.
            if ( oldCompilation != null && oldCompilation.ExternalReferences == newCompilation.ExternalReferences
                                        && this._cache.TryGetValue( oldCompilation, out var list ) )
            {
                return (list.CompilationVersion.References, ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange>.Empty);
            }

            var changeListBuilder = ImmutableDictionary.CreateBuilder<AssemblyIdentity, ReferencedCompilationChange>();
            var referenceListBuilder = ImmutableDictionary.CreateBuilder<AssemblyIdentity, ICompilationVersion>();

            var oldReferences = oldCompilation?.ExternalReferences.OfType<CompilationReference>()
                .ToDictionary( x => x.Compilation.Assembly.Identity, x => x.Compilation );

            var compilationReferences = newCompilation.ExternalReferences.OfType<CompilationReference>().ToList();

            foreach ( var reference in compilationReferences )
            {
                ReferencedCompilationChange changes;
                ICompilationVersion compilationVersion;

                var assemblyIdentity = reference.Compilation.Assembly.Identity;

                if ( oldCompilation != null && oldReferences!.TryGetValue( assemblyIdentity, out var oldReferenceCompilation ) )
                {
                    var compilationChanges = await this.GetCompilationChangesAsyncCore(
                        oldReferenceCompilation,
                        reference.Compilation,
                        semaphoreOwned,
                        cancellationToken );

                    compilationVersion = compilationChanges.NewCompilationVersion;

                    if ( compilationChanges.HasChange )
                    {
                        changes = new ReferencedCompilationChange(
                            oldReferenceCompilation,
                            reference.Compilation,
                            ReferencedCompilationChangeKind.Modified,
                            compilationChanges );
                    }
                    else
                    {
                        // No change.
                        changes = default;
                    }
                }
                else
                {
                    // If there is no old compilation, the reference is new.
                    changes = new ReferencedCompilationChange( null, reference.Compilation, ReferencedCompilationChangeKind.Added );
                    compilationVersion = await this.GetCompilationVersionCoreAsync( null, reference.Compilation, semaphoreOwned, cancellationToken );
                }

                if ( changes.ChangeKind != ReferencedCompilationChangeKind.None )
                {
                    changeListBuilder.Add( assemblyIdentity, changes );
                }

                referenceListBuilder.Add( assemblyIdentity, compilationVersion );
            }

            // Check removed references.
            if ( oldCompilation != null )
            {
                var referencedAssemblyIdentifies = new HashSet<AssemblyIdentity>( compilationReferences.Select( x => x.Compilation.Assembly.Identity ) );

                foreach ( var reference in oldReferences! )
                {
                    if ( !referencedAssemblyIdentifies.Contains( reference.Key ) )
                    {
                        changeListBuilder.Add(
                            reference.Key,
                            new ReferencedCompilationChange( reference.Value, null, ReferencedCompilationChangeKind.Removed ) );
                    }
                }
            }

            return (referenceListBuilder.ToImmutable(), changeListBuilder.ToImmutable());
        }

        public async ValueTask<CompilationChanges> GetCompilationChangesAsyncCore(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
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

                if ( !semaphoreOwned )
                {
                    await this._semaphore.WaitAsync( cancellationToken );
                }

                try
                {
                    if ( !this._cache.TryGetValue( newCompilation, out newList ) )
                    {
                        var referencedCompilationChanges = await this.GetReferencesAsync( oldCompilation, newCompilation, true, cancellationToken );

                        var compilationVersion = CompilationVersion.Create(
                            newCompilation,
                            GetDiffStrategy(),
                            referencedCompilationChanges.References,
                            cancellationToken );

                        newList = new ChangeLinkedList( compilationVersion );
                        this._cache.Add( newCompilation, newList );
                    }

                    return newList.NonIncrementalChanges;
                }
                finally
                {
                    if ( !semaphoreOwned )
                    {
                        this._semaphore.Release();
                    }
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
                        semaphoreOwned,
                        cancellationToken );

                    var changesFromClosestCompilation = CompilationChanges.Incremental(
                        closestIncrementalChanges.NewCompilationVersion,
                        newCompilation,
                        references.References,
                        references.Changes,
                        cancellationToken );

                    incrementalChanges = await this.MergeChangesCoreAsync(
                        closestIncrementalChanges,
                        changesFromClosestCompilation,
                        semaphoreOwned,
                        cancellationToken );

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

                    if ( !semaphoreOwned )
                    {
                        await this._semaphore.WaitAsync( cancellationToken );
                    }

                    try
                    {
                        if ( !this._cache.TryGetValue( oldCompilation, out var changeLinkedListFromOldCompilation ) )
                        {
                            // We have never processed the old compilation, so we have to compute it from the scratch.

                            var oldReferences = await this.GetReferencesAsync( null, oldCompilation, true, cancellationToken );

                            var oldCompilationVersion = CompilationVersion.Create(
                                oldCompilation,
                                GetDiffStrategy(),
                                oldReferences.References,
                                cancellationToken );

                            changeLinkedListFromOldCompilation = new ChangeLinkedList( oldCompilationVersion );
                            this._cache.Add( oldCompilation, changeLinkedListFromOldCompilation );
                        }

                        var newReferences = await this.GetReferencesAsync(
                            changeLinkedListFromOldCompilation.CompilationVersion.Compilation,
                            newCompilation,
                            true,
                            cancellationToken );

                        // Compute the increment.
                        incrementalChanges = CompilationChanges.Incremental(
                            changeLinkedListFromOldCompilation.CompilationVersion,
                            newCompilation,
                            newReferences.References,
                            newReferences.Changes,
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
                        if ( !semaphoreOwned )
                        {
                            this._semaphore.Release();
                        }
                    }
                }
            }
        }

        public async ValueTask<CompilationChanges> MergeChangesCoreAsync(
            CompilationChanges first,
            CompilationChanges second,
            bool semaphoreOwned,
            CancellationToken cancellationToken )
        {
            if ( !first.HasChange || !second.IsIncremental )
            {
                return second;
            }
            else if ( !second.HasChange )
            {
                return first;
            }
            else
            {
                this._metalamaDiffStrategy.Observer?.OnMergeCompilationChanges();

                // Merge syntax tree changes.
                var mergedSyntaxTreeBuilder = first.SyntaxTreeChanges.ToBuilder();

                foreach ( var syntaxTreeChanges in second.SyntaxTreeChanges )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( !mergedSyntaxTreeBuilder.TryGetValue( syntaxTreeChanges.Key, out var oldSyntaxTreeChange ) )
                    {
                        mergedSyntaxTreeBuilder.Add( syntaxTreeChanges );
                    }
                    else
                    {
                        var merged = oldSyntaxTreeChange.Merge( syntaxTreeChanges.Value );

                        if ( merged.SyntaxTreeChangeKind == SyntaxTreeChangeKind.None )
                        {
                            mergedSyntaxTreeBuilder.Remove( syntaxTreeChanges );
                        }
                        else
                        {
                            mergedSyntaxTreeBuilder[syntaxTreeChanges.Key] = merged;
                        }
                    }
                }

                // Merge changes in referenced compilations.
                var mergedReferencedCompilationBuilder = first.ReferencedCompilationChanges.ToBuilder();

                foreach ( var referencedCompilationChange in second.ReferencedCompilationChanges )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( !mergedReferencedCompilationBuilder.TryGetValue( referencedCompilationChange.Key, out var oldReferencedCompilationChange ) )
                    {
                        mergedReferencedCompilationBuilder.Add( referencedCompilationChange );
                    }
                    else
                    {
                        var merged = await this.MergeChangesCoreAsync(
                            oldReferencedCompilationChange,
                            referencedCompilationChange.Value,
                            semaphoreOwned,
                            cancellationToken );
                        mergedReferencedCompilationBuilder.Add( referencedCompilationChange.Key, merged );
                    }
                }

                return new CompilationChanges(
                    first.OldCompilationVersion,
                    second.NewCompilationVersion,
                    mergedSyntaxTreeBuilder.ToImmutable(),
                    mergedReferencedCompilationBuilder.ToImmutable(),
                    first.HasCompileTimeCodeChange | second.HasCompileTimeCodeChange,
                    first.IsIncremental );
            }
        }

        public async ValueTask<ReferencedCompilationChange> MergeChangesCoreAsync(
            ReferencedCompilationChange first,
            ReferencedCompilationChange second,
            bool semaphoreOwned,
            CancellationToken cancellationToken = default )
        {
            switch (first.ChangeKind, second.ChangeKind)
            {
                case (_, ReferencedCompilationChangeKind.None):
                    return first;

                case (ReferencedCompilationChangeKind.None, _):
                    return second;

                case (ReferencedCompilationChangeKind.Removed, ReferencedCompilationChangeKind.Added):
                    {
                        var changes = await this.GetCompilationChangesAsyncCore(
                            first.OldCompilation.AssertNotNull(),
                            second.NewCompilation.AssertNotNull(),
                            semaphoreOwned,
                            cancellationToken );

                        return new ReferencedCompilationChange(
                            first.OldCompilation,
                            second.NewCompilation,
                            ReferencedCompilationChangeKind.Modified,
                            changes );
                    }

                case (ReferencedCompilationChangeKind.Added, ReferencedCompilationChangeKind.Removed):
                    return new ReferencedCompilationChange( first.NewCompilation, first.OldCompilation, ReferencedCompilationChangeKind.None );

                case (ReferencedCompilationChangeKind.Modified, ReferencedCompilationChangeKind.Modified):
                    {
                        var changes = await this.MergeChangesCoreAsync( first.Changes!, second.Changes!, semaphoreOwned, cancellationToken );

                        return changes.HasChange
                            ? new ReferencedCompilationChange(
                                first.OldCompilation,
                                first.NewCompilation,
                                ReferencedCompilationChangeKind.Modified,
                                changes )
                            : new ReferencedCompilationChange( first.NewCompilation, first.OldCompilation, ReferencedCompilationChangeKind.None );
                    }

                default:
                    throw new AssertionFailedException();
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
}