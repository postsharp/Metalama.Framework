// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferencedSymbolInfo
{
    private ConcurrentDictionary<ISymbol, ReferencingNodeList>? _explicitReferences;
    private ConcurrentBag<ReferencedSymbolChild>? _children;

    public ISymbol ReferencedSymbol { get; }

    internal ReferencedSymbolInfo( ISymbol referencedSymbol )
    {
        this.ReferencedSymbol = referencedSymbol;
    }

    internal void AddChild( ReferencedSymbolInfo child, ChildKinds kind )
    {
        LazyInitializer.EnsureInitialized( ref this._children, static () => new ConcurrentBag<ReferencedSymbolChild>() );

        this._children.Add( new ReferencedSymbolChild( child, kind ) );
    }

    internal void AddReference( ISymbol referencingSymbol, SyntaxNodeOrToken node, ReferenceKinds referenceKinds )
    {
        LazyInitializer.EnsureInitialized( ref this._explicitReferences, static () => new ConcurrentDictionary<ISymbol, ReferencingNodeList>() );

        var nodes = this._explicitReferences.GetOrAddNew( referencingSymbol );

        lock ( nodes )
        {
            nodes.Add( new ReferencingNode( node, referenceKinds ) );
        }
    }

    public IEnumerable<ReferencingSymbolInfo> References
        => this._explicitReferences?.SelectAsReadOnlyCollection( x => new ReferencingSymbolInfo( x.Key, x.Value ) )
           ?? Enumerable.Empty<ReferencingSymbolInfo>();

    private IEnumerable<ReferencedSymbolInfo> Children( ChildKinds kinds )
        => this._children?.Where( x => (x.Kind & kinds) != 0 ).Select( x => x.Info ) ?? Enumerable.Empty<ReferencedSymbolInfo>();

    private IEnumerable<ReferencedSymbolInfo> DescendantsAndSelf( ChildKinds kinds ) => this.SelectManyRecursiveDistinct( x => x.Children( kinds ) );

    public IEnumerable<ReferencingSymbolInfo> GetAllReferences( ChildKinds kinds ) => this.DescendantsAndSelf( kinds ).SelectMany( x => x.References );
}