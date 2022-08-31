// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Build a <see cref="SyntaxList{TNode}"/> from an original syntax list where some nodes have been skipped,
/// while preserving important trivia.
/// </summary>
/// <typeparam name="T">Type of syntax nodes.</typeparam>
internal class IncompleteSyntaxListBuilder
{
    private readonly List<SyntaxNodeOrTriviaList> _items;

    public int NodeCount { get; private set; }

    public IncompleteSyntaxListBuilder( int capacity = 4 )
    {
        this._items = new List<SyntaxNodeOrTriviaList>( capacity );
    }

    public void Add( SyntaxNode node )
    {
        this.NodeCount++;
        this._items.Add( new SyntaxNodeOrTriviaList( node ) );
    }

    public void AddRange( IEnumerable<SyntaxNode> nodes )
    {
        foreach ( var node in nodes )
        {
            this.Add( node );
        }
    }

    public void Skip( SyntaxNode node )
    {
        if ( node.HasLeadingTrivia )
        {
            this._items.Add( new SyntaxNodeOrTriviaList( node.GetLeadingTrivia() ) );
        }

        if ( node.HasTrailingTrivia )
        {
            this._items.Add( new SyntaxNodeOrTriviaList( node.GetTrailingTrivia() ) );
        }
    }

    public IReadOnlyList<T> DequeueNodesOfType<T>() where T : SyntaxNode
    {
        var outputList = new List<T>();

        var pendingTrivia = SyntaxTriviaList.Empty;

        var numberOfItemsDequeued = 0;

        foreach ( var item in this._items )
        {
            if ( item.Node == null )
            {
                numberOfItemsDequeued++;

                pendingTrivia = pendingTrivia.AddRange( item.TriviaList.Where( MustKeepTrivia ) );
            }
            else
            {
                // If we have a node of a different type than expected, do not deque it.
                if ( item.Node.AssertNotNull() is not T node )
                {
                    break;
                }

                numberOfItemsDequeued++;

                // Add any trivia before.
                if ( pendingTrivia.Count > 0 )
                {
                    node = node.WithLeadingTrivia( node.GetLeadingTrivia().AddRange( pendingTrivia ) );
                    pendingTrivia = SyntaxTriviaList.Empty;
                }

                outputList.Add( node );
            }
        }

        if ( pendingTrivia.Count > 0 && outputList.Count > 0 )
        {
            var lastNode = outputList[outputList.Count - 1];
            outputList[outputList.Count - 1] = lastNode.WithTrailingTrivia( lastNode.GetTrailingTrivia().AddRange( pendingTrivia ) );
            pendingTrivia = SyntaxTriviaList.Empty;
        }

        this._items.RemoveRange( 0, numberOfItemsDequeued );

        if ( pendingTrivia.Count > 0 )
        {
            this._items.Insert( 0, new SyntaxNodeOrTriviaList( pendingTrivia ) );
        }

        return outputList;
    }

    public SyntaxTriviaList DequeueTriviaList()
    {
#if DEBUG
        if ( this._items.Any( x => x.Node != null ) )
        {
            throw new AssertionFailedException();
        }
#endif

        var triviaList = SyntaxTriviaList.Empty;
        var numberOfItemsDequeued = 0;

        foreach ( var item in this._items )
        {
            if ( item.Node == null )
            {
                numberOfItemsDequeued++;
                triviaList = triviaList.AddRange( item.TriviaList );
            }
        }
        
        this._items.RemoveRange( 0, numberOfItemsDequeued );

        return triviaList;
    }

    private static bool MustKeepTrivia( SyntaxTrivia trivia )
        => trivia.Kind() switch
        {
            SyntaxKind.DefineDirectiveTrivia => true,
            SyntaxKind.IfDirectiveTrivia => true,
            SyntaxKind.ElifDirectiveTrivia => true,
            SyntaxKind.ElseDirectiveTrivia => true,
            SyntaxKind.ErrorDirectiveTrivia => true,
            SyntaxKind.NullableDirectiveTrivia => true,
            SyntaxKind.RegionDirectiveTrivia => true,
            SyntaxKind.UndefDirectiveTrivia => true,
            SyntaxKind.WarningDirectiveTrivia => true,
            SyntaxKind.EndIfDirectiveTrivia => true,
            SyntaxKind.EndRegionDirectiveTrivia => true,
            SyntaxKind.PragmaChecksumDirectiveTrivia => true,
            SyntaxKind.PragmaWarningDirectiveTrivia => true,
            _ => false
        };

    /// <summary>
    /// Filters the nodes using a function that can return <c>null</c> if the node needs to be suppressed, and preserves the
    /// trivia of suppressed nodes.
    /// </summary>
    public void AddFilteredNodes<T>( IEnumerable<T> nodes, Func<T, T?> transform )
        where T : SyntaxNode
    {
        foreach ( var node in nodes )
        {
            var transformed = transform( node );

            if ( transformed != null )
            {
                this.Add( transformed );
            }
            else
            {
                this.Skip( node );
            }
        }
    }

    public bool ContainsNode<T>( Predicate<T> predicate ) where T : SyntaxNode => this._items.Any( x => x.Node is T node && predicate( node ) );

    private struct SyntaxNodeOrTriviaList
    {
        public SyntaxNode? Node { get; }

        public SyntaxTriviaList TriviaList { get; }

        public SyntaxNodeOrTriviaList( SyntaxNode? node )
        {
            this.Node = node;
            this.TriviaList = default;
        }

        public SyntaxNodeOrTriviaList( SyntaxTriviaList triviaList )
        {
            this.Node = null;
            this.TriviaList = triviaList;
        }
    }
}