// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaProjectTracker
{
    private readonly SemaphoreSlim _semaphore = new( 1 );
    private readonly IncrementalChangeTracker _incrementalChangeTracker = new();
    private CompilationChangeTracker _tracker;

    public NonMetalamaProjectTracker( ProjectKey projectKey, IServiceProvider serviceProvider )
    {
        this.ProjectKey = projectKey;
        this._tracker = new CompilationChangeTracker( new CompilationChangeTrackerStrategy( serviceProvider, false, true ) );
    }

    public ProjectKey ProjectKey { get; }

    public async ValueTask<DesignTimeCompilationReference> GetCompilationReferenceAsync(
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        var tracker = this._tracker;

        if ( tracker.LastCompilation != newCompilation )
        {
            await this._semaphore.WaitAsync( cancellationToken );

            tracker = this._tracker;

            if ( tracker.LastCompilation != newCompilation )
            {
                try
                {
                    var newTracker = tracker.Update( newCompilation, DependencyChanges.Empty, cancellationToken );
                    var changes = newTracker.UnprocessedChanges.AssertNotNull();
                    this._tracker = tracker = newTracker;

                    if ( tracker.LastCompilation != null )
                    {
                        this._incrementalChangeTracker.Add( tracker.LastCompilation, tracker.LastCompilation, changes );
                    }
                }
                finally
                {
                    this._semaphore.Release();
                }
            }
        }

        return new DesignTimeCompilationReference(
            new CompilationVersion( newCompilation, 0, tracker.LastTrees.AssertNotNull() ),
            newCompilation,
            ( fromCompilation, toCompilation, ct )
                => this._incrementalChangeTracker.FindIncrementalChanges( fromCompilation, toCompilation, ct ) );
    }
}