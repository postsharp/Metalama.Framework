// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.AspectOrdering
{
    /// <summary>
    /// Implements algorithms for graphs.
    /// </summary>
    internal abstract class AbstractGraph
    {
        public const int NotDiscovered = int.MaxValue;
        public const int Cycle = int.MinValue;

        private readonly int _size;

        protected AbstractGraph( int size )
        {
            this._size = size;
        }

        public int[] GetInitialVector()
        {
            var n = this._size;
            var vector = new int[n];

            for ( var i = 0; i < n; i++ )
            {
                vector[i] = NotDiscovered;
            }

            return vector;
        }

        public abstract void AddEdge( int predecessor, int successor );

        public abstract void RemoveEdge( int predecessor, int successor );

        public abstract bool HasEdge( int predecessor, int successor );

        public abstract int DoBreadthFirstSearch( int initialNode, int[] distances, int[] directPredecessors );
    }
}