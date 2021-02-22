using System.Collections.Generic;

#nullable enable

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal sealed class Graph : AbstractGraph
    {
        private readonly SimpleLinkedListNode<int>?[] _successors;
        private readonly SimpleLinkedListNode<int>?[] _predecessors;

        public Graph( int size ) : base( size )
        {
            this._successors = new SimpleLinkedListNode<int>[size];
            this._predecessors = new SimpleLinkedListNode<int>[size];
        }

        public override void AddEdge( int predecessor, int successor )
        {
            SimpleLinkedListNode<int>.Insert( ref this._successors[predecessor], successor );
            SimpleLinkedListNode<int>.Insert( ref this._predecessors[successor], predecessor );
        }

        public override void RemoveEdge( int predecessor, int successor )
        {
            _ = SimpleLinkedListNode<int>.Remove( ref this._successors[predecessor], successor );
            _ = SimpleLinkedListNode<int>.Remove( ref this._predecessors[successor], predecessor );
        }

        public override bool HasEdge( int predecessor, int successor )
        {
            var current = this._successors[predecessor];

            while ( current != null )
            {
                if ( current.Value == successor )
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        public override int DoBreadthFirstSearch( int initialNode, int[] distances, int[] directPredecessors )
        {
            var n = distances.Length;

            distances[initialNode] = 0;

            Queue<NodeInfo> queue = new( n );
            queue.Enqueue( new NodeInfo {Node = initialNode, NodesInPath = new SimpleLinkedListNode<int>( initialNode, null )} );


            while ( queue.Count > 0 )
            {
                var nodeInfo = queue.Dequeue();
                var current = nodeInfo.Node;
                var successorNode = this._successors[current];
                var currentDistance = distances[current];

                while ( successorNode != null )
                {
                    var successor = successorNode.Value;
                    var successorDistance = distances[successor];
                    var newSucccessorDistance = currentDistance + 1;

                    // Check that the new node is not already in the path.
                    var hasCycle = false;
                    var nodeInPathCursor = nodeInfo.NodesInPath;
                    while ( nodeInPathCursor != null )
                    {
                        if ( nodeInPathCursor.Value == successor )
                        {
                            hasCycle = true;
                            break;
                        }

                        nodeInPathCursor = nodeInPathCursor.Next;
                    }

                    if ( hasCycle )
                    {
                        // We just discovered that the successor is part of a cycle.
                        distances[successor] = Cycle;
                        directPredecessors[successor] = current;
                        return successor;
                    }

                    if ( successorDistance == NotDiscovered || successorDistance < newSucccessorDistance )
                    {
                        distances[successor] = newSucccessorDistance;
                        directPredecessors[successor] = current;

                        queue.Enqueue( new NodeInfo {Node = successor, NodesInPath = new SimpleLinkedListNode<int>( successor, nodeInfo.NodesInPath )} );
                    }
                    else if ( successorDistance == Cycle )
                    {
                        // We have already discovered that the successor is part of a cycle.
                    }

                    successorNode = successorNode.Next;
                }
            }

            return -1;
        }

        private struct NodeInfo
        {
            public int Node;
            public SimpleLinkedListNode<int> NodesInPath;
        }
    }
}