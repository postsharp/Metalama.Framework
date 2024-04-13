// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Caching;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Fabrics;

internal class DeclarationSelectionContext
{
    private static readonly WeakCache<CompilationModel, ConcurrentDictionary<IAspectReceiver<IDeclaration>, Node>> _staticCache = new();
    private ConcurrentDictionary<IAspectReceiver<IDeclaration>, Node>? _selectionCache;

    public CancellationToken CancellationToken { get; }

    public CompilationModel Compilation { get; }

    private class Node
    {
        public SemaphoreSlim Semaphore { get; } = new( 1 );

        public object? Payload;
    }

    public DeclarationSelectionContext( CompilationModel compilation, CancellationToken cancellationToken )
    {
        this.CancellationToken = cancellationToken;
        this.Compilation = compilation;
    }

    public async ValueTask<T?> GetFromCacheAsync<T>( IAspectReceiver<IDeclaration> receiver, CancellationToken cancellationToken )
        where T : class
    {
        this._selectionCache ??= _staticCache.GetOrAdd( this.Compilation, _ => new ConcurrentDictionary<IAspectReceiver<IDeclaration>, Node>() );

        var node = this._selectionCache.GetOrAdd( receiver, r => new Node() );

        if ( node.Payload != null )
        {
            return (T) node.Payload;
        }
        else
        {
            await node.Semaphore.WaitAsync( cancellationToken );

            return null;
        }
    }

    public void AddToCache( IAspectReceiver<IDeclaration> receiver, object payload )
    {
        if ( !this._selectionCache.TryGetValue( receiver, out var node ) )
        {
            throw new AssertionFailedException();
        }

        node.Payload = payload;
        node.Semaphore.Release();
    }
}