// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

internal sealed partial class ProjectVersionProvider
{
    private sealed partial class Implementation : IDisposable
    {
        private readonly ConditionalWeakTable<Compilation, ChangeLinkedList> _cache = new();
        private readonly Dictionary<ProjectKey, WeakReference<Compilation>> _lastCompilationPerProject = new();
        private readonly SemaphoreSlim _semaphore = new( 1 );
        private readonly DiffStrategy _metalamaDiffStrategy;
        private readonly DiffStrategy _nonMetalamaDiffStrategy;
        private readonly IMetalamaProjectClassifier _metalamaProjectClassifier;

        public Implementation( GlobalServiceProvider serviceProvider, bool isTest )
        {
            var observer = serviceProvider.GetService<IDifferObserver>();
            this._metalamaDiffStrategy = new DiffStrategy( isTest, true, true, observer );
            this._nonMetalamaDiffStrategy = new DiffStrategy( isTest, false, true, observer );
            this._metalamaProjectClassifier = serviceProvider.GetRequiredService<IMetalamaProjectClassifier>();
        }

        /// <summary>
        /// Computes an incremental <see cref="CompilationChanges"/> between an old compilation and a new compilation
        /// based on the values from the cache only, or returns <c>null</c> if the value was not found in the cache.
        /// </summary>
        private bool TryGetIncrementalChangesFromCache(
            Compilation oldCompilation,
            Compilation newCompilation,
            TestableCancellationToken cancellationToken,
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

            // If the new compilation is exactly the old compilation, there is no change.
            if ( newCompilation == oldCompilation )
            {
                exactChanges = CompilationChanges.Empty( list.ProjectVersion, list.ProjectVersion );
                closestChanges = null;

                return true;
            }

            // Find an available incremental change in the list.
            for ( var node = list.FirstIncrementalChange; node != null; node = node.Next )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( node.IncrementalChanges.NewProjectVersion.Compilation == newCompilation )
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

        public async ValueTask<ProjectVersion> GetCompilationVersionCoreAsync(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
            TestableCancellationToken cancellationToken = default )
        {
            // When we are asked a CompilationVersion, we do it through getting a CompilationChanges, because this path is incremental
            // and offers optimal performances.
            var changes = await this.GetCompilationChangesAsyncCoreAsync( oldCompilation, newCompilation, semaphoreOwned, cancellationToken );

            return changes.NewProjectVersion;
        }

        private async ValueTask<ReferenceChanges> GetReferencesAsync(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
            TestableCancellationToken cancellationToken )
        {
            // If references are the same by reference, there is no need to compare anything. Most hits to the method should take this shortcut.
            if ( oldCompilation != null && oldCompilation.ExternalReferences == newCompilation.ExternalReferences
                                        && this._cache.TryGetValue( oldCompilation, out var list ) )
            {
                return new ReferenceChanges(
                    list.ProjectVersion.ReferencedProjectVersions,
                    ImmutableDictionary<ProjectKey, ReferencedProjectChange>.Empty,
                    list.ProjectVersion.ReferencesPortableExecutables,
                    ImmutableDictionary<string, ReferenceChangeKind>.Empty );
            }

            var projectReferences = await this.GetProjectReferencesAsync( oldCompilation, newCompilation, semaphoreOwned, cancellationToken );
            var portableExecutableReferences = GetPortableExecutableReferences( oldCompilation, newCompilation );

            return new ReferenceChanges(
                projectReferences.References,
                projectReferences.Changes,
                portableExecutableReferences.References,
                portableExecutableReferences.Changes );
        }

        private async Task<(ImmutableDictionary<ProjectKey, ReferencedProjectChange> Changes, ImmutableDictionary<ProjectKey, IProjectVersion> References)>
            GetProjectReferencesAsync(
                Compilation? oldCompilation,
                Compilation newCompilation,
                bool semaphoreOwned,
                TestableCancellationToken cancellationToken )
        {
            // Verify changes in referenced projects.
            var changeListBuilder = ImmutableDictionary.CreateBuilder<ProjectKey, ReferencedProjectChange>();
            var referenceListBuilder = ImmutableDictionary.CreateBuilder<ProjectKey, IProjectVersion>();

            var oldProjectReferences = oldCompilation?.ExternalReferences.OfType<CompilationReference>()
                .ToDictionary( x => x.Compilation.GetProjectKey(), x => x.Compilation );

            var newProjectReferences = newCompilation.ExternalReferences.OfType<CompilationReference>().ToList();

            foreach ( var reference in newProjectReferences )
            {
                ReferencedProjectChange changes;
                IProjectVersion projectVersion;

                cancellationToken.ThrowIfCancellationRequested();

                var assemblyIdentity = reference.Compilation.GetProjectKey();

                if ( oldCompilation != null && oldProjectReferences!.TryGetValue( assemblyIdentity, out var oldReferenceCompilation ) )
                {
                    var compilationChanges = await this.GetCompilationChangesAsyncCoreAsync(
                        oldReferenceCompilation,
                        reference.Compilation,
                        semaphoreOwned,
                        cancellationToken );

                    projectVersion = compilationChanges.NewProjectVersion;

                    if ( compilationChanges.HasChange )
                    {
                        changes = new ReferencedProjectChange(
                            oldReferenceCompilation,
                            reference.Compilation,
                            ReferenceChangeKind.Modified,
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
                    changes = new ReferencedProjectChange( null, reference.Compilation, ReferenceChangeKind.Added );
                    projectVersion = await this.GetCompilationVersionCoreAsync( null, reference.Compilation, semaphoreOwned, cancellationToken );
                }

                if ( changes.ChangeKind != ReferenceChangeKind.None )
                {
                    changeListBuilder.Add( assemblyIdentity, changes );
                }

                referenceListBuilder.Add( assemblyIdentity, projectVersion );
            }

            // Check removed references.
            if ( oldCompilation != null )
            {
                var referencedAssemblyIdentifies =
                    new HashSet<ProjectKey>( newProjectReferences.SelectAsImmutableArray( x => x.Compilation.GetProjectKey() ) );

                foreach ( var reference in oldProjectReferences! )
                {
                    if ( !referencedAssemblyIdentifies.Contains( reference.Key ) )
                    {
                        changeListBuilder.Add(
                            reference.Key,
                            new ReferencedProjectChange( reference.Value, null, ReferenceChangeKind.Removed ) );
                    }
                }
            }

            return (changeListBuilder.ToImmutable(), referenceListBuilder.ToImmutable());
        }

        private static (ImmutableDictionary<string, ReferenceChangeKind> Changes, ImmutableHashSet<string> References)
            GetPortableExecutableReferences(
                Compilation? oldCompilation,
                Compilation newCompilation )
        {
            var changeListBuilder = ImmutableDictionary.CreateBuilder<string, ReferenceChangeKind>( StringComparer.Ordinal );
            var referenceListBuilder = ImmutableHashSet.CreateBuilder<string>( StringComparer.Ordinal );

            var oldReferences = oldCompilation?.ExternalReferences.OfType<PortableExecutableReference>()
                .Select( r => r.FilePath )
                .WhereNotNull()
                .ToImmutableHashSet( StringComparer.Ordinal );

            var newReferences = newCompilation.ExternalReferences.OfType<PortableExecutableReference>()
                .Select( x => x.FilePath )
                .WhereNotNull()
                .ToImmutableHashSet( StringComparer.Ordinal );

            foreach ( var reference in newReferences )
            {
                if ( oldReferences == null || !oldReferences.Contains( reference ) )
                {
                    changeListBuilder.Add( reference, ReferenceChangeKind.Added );
                }
            }

            if ( oldCompilation != null )
            {
                foreach ( var reference in oldReferences! )
                {
                    if ( !newReferences.Contains( reference ) )
                    {
                        changeListBuilder.Add( reference, ReferenceChangeKind.Removed );
                    }
                }
            }

            return (changeListBuilder.ToImmutable(), referenceListBuilder.ToImmutable());
        }

        public async ValueTask<CompilationChanges> GetCompilationChangesAsyncCoreAsync(
            Compilation? oldCompilation,
            Compilation newCompilation,
            bool semaphoreOwned,
            TestableCancellationToken cancellationToken = default )
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
                    cancellationToken.ThrowIfCancellationRequested();
                    await this._semaphore.WaitAsync( cancellationToken );
                }

                try
                {
                    if ( !this._cache.TryGetValue( newCompilation, out newList ) )
                    {
                        var referencedCompilationChanges = await this.GetReferencesAsync( oldCompilation, newCompilation, true, cancellationToken );

                        var compilationVersion = ProjectVersion.Create(
                            newCompilation,
                            newCompilation.GetProjectKey(),
                            GetDiffStrategy(),
                            referencedCompilationChanges.NewProjectReferences,
                            referencedCompilationChanges.NewPortableExecutableReferences,
                            cancellationToken );

                        newList = new ChangeLinkedList( compilationVersion );
                        this._cache.Add( newCompilation, newList );

                        this._lastCompilationPerProject[newCompilation.GetProjectKey()] =
                            new WeakReference<Compilation>( newCompilation );
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
                        closestIncrementalChanges.NewProjectVersion.Compilation,
                        newCompilation,
                        semaphoreOwned,
                        cancellationToken );

                    var changesFromClosestCompilation = CompilationChanges.Incremental(
                        closestIncrementalChanges.NewProjectVersion,
                        newCompilation,
                        references,
                        cancellationToken );

                    incrementalChanges = await this.MergeCompilationChangesAsync(
                        closestIncrementalChanges,
                        changesFromClosestCompilation,
                        semaphoreOwned,
                        cancellationToken );

                    if ( this._cache.TryGetValue( oldCompilation, out var changeLinkedListFromOldCompilation ) )
                    {
                        changeLinkedListFromOldCompilation.Insert( incrementalChanges );
                    }

                    if ( !this._cache.TryGetValue( newCompilation, out _ ) )
                    {
                        this._cache.Add( newCompilation, new ChangeLinkedList( incrementalChanges.NewProjectVersion ) );
                    }
                    else
                    {
                        // For some (unknown) reason, the compilation was added somewhere else, while adding the cache. 
                        // I don't have an explanation at the moment of why this may happen.
                    }

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
                        ProjectVersion oldProjectVersion;

                        if ( this._cache.TryGetValue( oldCompilation, out var changeLinkedListFromOldCompilation ) )
                        {
                            oldProjectVersion = changeLinkedListFromOldCompilation.ProjectVersion;
                        }
                        else
                        {
                            // We have never processed the old compilation, so we have to compute it from the scratch.

                            // Get the last compilation we know for the project. It can help building the old CompilationVersion incrementally.
                            Compilation? lastCompilationOfProject = null;

                            if ( this._lastCompilationPerProject.TryGetValue(
                                    oldCompilation.GetProjectKey(),
                                    out var lastCompilationOfProjectRef ) )
                            {
                                _ = lastCompilationOfProjectRef.TryGetTarget( out lastCompilationOfProject );
                            }

                            var oldReferences = await this.GetReferencesAsync( lastCompilationOfProject, oldCompilation, true, cancellationToken );

                            if ( lastCompilationOfProject != null )
                            {
                                // Build oldCompilationVersion incrementally.
                                oldProjectVersion = await this.GetCompilationVersionCoreAsync(
                                    lastCompilationOfProject,
                                    oldCompilation,
                                    true,
                                    cancellationToken );

                                if ( !this._cache.TryGetValue( oldCompilation, out changeLinkedListFromOldCompilation ) )
                                {
                                    throw new AssertionFailedException( $"Compilation '{oldCompilation.Assembly.Identity}' not found in cache." );
                                }
                            }
                            else
                            {
                                // Build oldCompilationVersion from scratch.
                                oldProjectVersion = ProjectVersion.Create(
                                    oldCompilation,
                                    oldCompilation.GetProjectKey(),
                                    GetDiffStrategy(),
                                    oldReferences.NewProjectReferences,
                                    oldReferences.NewPortableExecutableReferences,
                                    cancellationToken );

                                changeLinkedListFromOldCompilation = new ChangeLinkedList( oldProjectVersion );
                                this._cache.Add( oldCompilation, changeLinkedListFromOldCompilation );
                            }
                        }

                        var referenceChanges = await this.GetReferencesAsync(
                            oldCompilation,
                            newCompilation,
                            true,
                            cancellationToken );

                        // Compute the increment.
                        incrementalChanges = CompilationChanges.Incremental(
                            oldProjectVersion,
                            newCompilation,
                            referenceChanges,
                            cancellationToken );

                        if ( !this._cache.TryGetValue( newCompilation, out _ ) )
                        {
                            this._cache.Add( newCompilation, new ChangeLinkedList( incrementalChanges.NewProjectVersion ) );

                            this._lastCompilationPerProject[newCompilation.GetProjectKey()] =
                                new WeakReference<Compilation>( newCompilation );
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

        private async ValueTask<CompilationChanges> MergeCompilationChangesAsync(
            CompilationChanges first,
            CompilationChanges second,
            bool semaphoreOwned,
            TestableCancellationToken cancellationToken )
        {
            if ( !first.HasChange || !second.IsIncremental )
            {
                return new CompilationChanges(
                    first.OldCompilationVersion,
                    second.NewProjectVersion,
                    second.SyntaxTreeChanges,
                    second.ReferencedCompilationChanges,
                    second.ReferencedPortableExecutableChanges,
                    second.HasCompileTimeCodeChange,
                    second.IsIncremental );
            }
            else if ( !second.HasChange )
            {
                return new CompilationChanges(
                    first.OldCompilationVersion,
                    second.NewProjectVersion,
                    first.SyntaxTreeChanges,
                    first.ReferencedCompilationChanges,
                    first.ReferencedPortableExecutableChanges,
                    first.HasCompileTimeCodeChange,
                    first.IsIncremental );
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
                        var merged = await this.MergeReferencedProjectChangesAsync(
                            oldReferencedCompilationChange,
                            referencedCompilationChange.Value,
                            semaphoreOwned,
                            cancellationToken );

                        mergedReferencedCompilationBuilder[referencedCompilationChange.Key] = merged;
                    }
                }

                // Merge changes in referenced portable executable.
                var mergedReferencedPortableExecutablesBuilder = first.ReferencedPortableExecutableChanges.ToBuilder();

                foreach ( var referencedPortableExecutableChange in second.ReferencedPortableExecutableChanges )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( !mergedReferencedPortableExecutablesBuilder.TryGetValue( referencedPortableExecutableChange.Key, out var oldChange ) )
                    {
                        mergedReferencedPortableExecutablesBuilder.Add( referencedPortableExecutableChange );
                    }
                    else
                    {
                        var merged = MergePortableExecutableChanges(
                            oldChange,
                            referencedPortableExecutableChange.Value );

                        mergedReferencedPortableExecutablesBuilder[referencedPortableExecutableChange.Key] = merged;
                    }
                }

                return new CompilationChanges(
                    first.OldCompilationVersion,
                    second.NewProjectVersion,
                    mergedSyntaxTreeBuilder.ToImmutable(),
                    mergedReferencedCompilationBuilder.ToImmutable(),
                    mergedReferencedPortableExecutablesBuilder.ToImmutable(),
                    first.HasCompileTimeCodeChange | second.HasCompileTimeCodeChange,
                    first.IsIncremental );
            }
        }

        private async ValueTask<ReferencedProjectChange> MergeReferencedProjectChangesAsync(
            ReferencedProjectChange first,
            ReferencedProjectChange second,
            bool semaphoreOwned,
            TestableCancellationToken cancellationToken = default )
        {
            switch (first.ChangeKind, second.ChangeKind)
            {
                case (_, ReferenceChangeKind.None):
                    return first;

                case (ReferenceChangeKind.None, _):
                    return second;

                case (ReferenceChangeKind.Removed, ReferenceChangeKind.Added):
                    {
                        var changes = await this.GetCompilationChangesAsyncCoreAsync(
                            first.OldCompilation.AssertNotNull(),
                            second.NewCompilation.AssertNotNull(),
                            semaphoreOwned,
                            cancellationToken );

                        return new ReferencedProjectChange(
                            first.OldCompilation,
                            second.NewCompilation,
                            ReferenceChangeKind.Modified,
                            changes );
                    }

                case (ReferenceChangeKind.Added, ReferenceChangeKind.Removed):
                    return new ReferencedProjectChange( first.NewCompilation, first.OldCompilation, ReferenceChangeKind.None );

                case (ReferenceChangeKind.Modified, ReferenceChangeKind.Modified):
                    {
                        var changes = await this.MergeCompilationChangesAsync( first.Changes!, second.Changes!, semaphoreOwned, cancellationToken );

                        return changes.HasChange
                            ? new ReferencedProjectChange(
                                first.OldCompilation,
                                first.NewCompilation,
                                ReferenceChangeKind.Modified,
                                changes )
                            : new ReferencedProjectChange( first.NewCompilation, first.OldCompilation, ReferenceChangeKind.None );
                    }

                case (ReferenceChangeKind.Added, ReferenceChangeKind.Modified):
                    return first;

                case (ReferenceChangeKind.Modified, ReferenceChangeKind.Removed):
                    return second;

                default:
                    throw new AssertionFailedException( $"Unexpected combination: ({first.ChangeKind}, {second.ChangeKind})" );
            }
        }

        private static ReferenceChangeKind MergePortableExecutableChanges(
            ReferenceChangeKind first,
            ReferenceChangeKind second )
        {
            switch (first, second)
            {
                case (_, ReferenceChangeKind.None):
                    return first;

                case (ReferenceChangeKind.None, _):
                    return second;

                case (ReferenceChangeKind.Removed, ReferenceChangeKind.Added):
                    return default;

                case (ReferenceChangeKind.Added, ReferenceChangeKind.Removed):
                    return default;

                case (ReferenceChangeKind.Added, ReferenceChangeKind.Modified):
                    return first;

                case (ReferenceChangeKind.Modified, ReferenceChangeKind.Removed):
                    return second;

                default:
                    throw new AssertionFailedException( $"Unexpected combination: ({first}, {second})" );
            }
        }

        public void Dispose()
        {
            this._semaphore.Dispose();
        }
    }
}