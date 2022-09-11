// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class CompilationVersionGraph
{
    private readonly DiffStrategy _strategy;
    private readonly ConditionalWeakTable<Compilation, ChangeLinkedList> _compilations = new();
    private readonly SemaphoreSlim _semaphore = new( 1 );

    public CompilationVersionGraph( DiffStrategy strategy )
    {
        this._strategy = strategy;
    }

    private CompilationChanges? GetIncrementalChanges( Compilation oldCompilation, Compilation newCompilation, CancellationToken cancellationToken )
    {
        if ( !this._compilations.TryGetValue( oldCompilation, out var list ) )
        {
            return null;
        }

        for ( var node = list.FirstIncrementalChange; node != null; node = node.Next )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( node.NewCompilation == newCompilation )
            {
                return node.Changes;
            }
            else
            {
                var baseChanges = this.GetIncrementalChanges( node.NewCompilation, newCompilation, cancellationToken );

                if ( baseChanges != null )
                {
                    var mergedChanges = node.Changes.Merge( baseChanges.Value );
                    list.Insert( newCompilation, mergedChanges );

                    return mergedChanges;
                }
            }
        }

        return null;
    }

    public async ValueTask<CompilationVersion> GetCompilationVersion(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        if ( oldCompilation == null )
        {
            await this._semaphore.WaitAsync( cancellationToken );

            try
            {
                var compilationVersion = new CompilationVersion( this._strategy )
                    .Update( newCompilation, DependencyChanges.Empty, cancellationToken );

                this._compilations.Add( newCompilation, new ChangeLinkedList( compilationVersion ) );

                return compilationVersion;
            }
            finally
            {
                this._semaphore.Release();
            }
        }
        else
        {
            var incrementalChanges = this.GetIncrementalChanges( oldCompilation, newCompilation, cancellationToken );

            if ( incrementalChanges != null )
            {
                if ( !this._compilations.TryGetValue( newCompilation, out var newList ) )
                {
                    // If we have incremental changes, we must also have the list of the new compilation.
                    throw new AssertionFailedException();
                }

                return newList.CompilationVersion.WithChanges( incrementalChanges.Value );
            }
            else
            {
                // We could not get the changes from the cache, so we need to run the diff algorithm.

                await this._semaphore.WaitAsync( cancellationToken );

                try
                {
                    if ( this._compilations.TryGetValue( oldCompilation, out var list ) )
                    {
                        var newCompilationVersion = list.CompilationVersion.Update( newCompilation, DependencyChanges.Empty, cancellationToken );
                        this._compilations.Add( newCompilation, new ChangeLinkedList( newCompilationVersion ) );

                        return newCompilationVersion;
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }
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
#pragma warning disable SA1401 // Field should be private
        public readonly CompilationVersion CompilationVersion;
#pragma warning restore SA1401        

        public IncrementalChangeNode? FirstIncrementalChange { get; private set; }

        public ChangeLinkedList( CompilationVersion compilationVersion )
        {
            this.CompilationVersion = compilationVersion.ResetChanges();
        }

        public void Insert( Compilation newCompilation, CompilationChanges changes )
        {
            this.FirstIncrementalChange = new IncrementalChangeNode( newCompilation, changes, this.FirstIncrementalChange );
        }
    }

    private class IncrementalChangeNode
    {
        public Compilation NewCompilation { get; }

        public CompilationChanges Changes { get; }

        public IncrementalChangeNode( Compilation newCompilation, in CompilationChanges changes, IncrementalChangeNode? next )
        {
            this.NewCompilation = newCompilation;
            this.Changes = changes;
            this.Next = next;
        }

        public IncrementalChangeNode? Next { get; }
    }
}