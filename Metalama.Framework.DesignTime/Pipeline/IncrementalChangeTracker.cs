using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class IncrementalChangeTracker
{
    private readonly ConditionalWeakTable<Compilation, IncrementalChange> _incrementalChanges = new();

    public void Add( Compilation oldCompilation, Compilation newCompilation, CompilationChanges changes )
    {
        if ( !changes.IsIncremental )
        {
            return;
        }
        
        var newIncrementalChange = new IncrementalChange( newCompilation, changes );

        if ( !this._incrementalChanges.TryGetValue( oldCompilation, out var existingIncrementalChange ) )
        {
            existingIncrementalChange.Last().Next = newIncrementalChange;
        }
        else
        {
            this._incrementalChanges.Add( oldCompilation, newIncrementalChange );
        }
    }
    
    public CompilationChanges? FindIncrementalChanges( Compilation oldCompilation, Compilation newCompilation, CancellationToken cancellationToken )
    {
        if ( this._incrementalChanges.TryGetValue( oldCompilation, out var node ) )
        {
            for ( ; node != null; node = node.Next )
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if ( node.NewCompilation == newCompilation )
                {
                    return node.Changes;
                }
                else
                {
                    var baseChanges = this.FindIncrementalChanges( node.NewCompilation, newCompilation, cancellationToken );

                    if ( baseChanges != null )
                    {
                        return node.Changes.Merge( baseChanges );
                    }
                }
            }
        }

        return null;
    }
    
    private class IncrementalChange
    {
        public Compilation NewCompilation { get; }

        public CompilationChanges Changes { get; }

        public IncrementalChange( Compilation newCompilation, CompilationChanges changes )
        {
            this.NewCompilation = newCompilation;
            this.Changes = changes;
        }

        public IncrementalChange? Next { get; set; }

        public IncrementalChange Last() => this.Next?.Last() ?? this;
    }
}