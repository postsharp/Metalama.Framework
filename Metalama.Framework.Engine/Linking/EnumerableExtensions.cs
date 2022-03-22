// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metalama.Framework.Engine.Linking
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Orders the input in reverse topological order, where descendants preceed their ancestors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="getDescendants"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderByReverseTopology<T>(this IEnumerable<T> enumerable, Func<T, IReadOnlyList<T>> getDescendants)
            where T : class
        {            
            // Topological sort using stack-based DFS.
            // First find entry points (nodes without any incoming edge)
            var entryPoints = new HashSet<T>( enumerable );

            foreach ( var o in enumerable )
            {
                var descendants = getDescendants( o );
                foreach ( var e in descendants )
                {
                    entryPoints.Remove( e );
                }
            }

            // TODO: Implement and use stack with PeekRef.
            var stack = new Stack<(T Node, IReadOnlyList<T>? Descendants, int Index)>();
            var stackSet = new HashSet<T>();

            foreach ( var e in entryPoints )
            {
                stackSet.Add( e );
                stack.Push( (e, null, -1) );

                while ( stack.Count > 0 )
                {
                    var current = stack.Pop();

                    if (current.Descendants == null)
                    {
                        // Opening a new node.
                        current.Descendants = getDescendants(current.Node);

                        if ( current.Descendants != null && current.Descendants.Count > 0 )
                        {
                            current.Index = 0;
                            stack.Push( current );
                            stack.Push( (current.Descendants[0], null, -1) );

                            if (!stackSet.Contains( current.Descendants[0] ) )
                            {
                                // Dependency cycle.
                                throw new AssertionFailedException();
                            }

                            continue;
                        }
                        else
                        {
                            // Current node has no children, we are leaving it.
                            stackSet.Remove( current.Node );
                            yield return current.Node;
                        }
                    }
                    else
                    {
                        // Going to the next child.

                        if (current.Index + 1 < current.Descendants.Count )
                        {
                            current.Index++;
                            stack.Push( current );
                            stack.Push( (current.Descendants[current.Index], null, -1) );

                            if ( !stackSet.Contains( current.Descendants[current.Index] ) )
                            {
                                // Dependency cycle.
                                throw new AssertionFailedException();
                            }
                        }
                        else
                        {
                            // We've visited all children, leaving the node.
                            stackSet.Remove( current.Node );
                            yield return current.Node;
                        }
                    }
                }
            }
        }
    }
}