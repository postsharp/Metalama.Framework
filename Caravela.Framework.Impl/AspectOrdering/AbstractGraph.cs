// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Text;

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

        public string Serialize()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendFormat( CultureInfo.InvariantCulture, "{0};", this._size );

            for ( var i = 0; i < this._size; i++ )
            {
                for ( var j = 0; j < this._size; j++ )
                {
                    if ( i == j )
                    {
                        continue;
                    }

                    if ( this.HasEdge( i, j ) )
                    {
                        stringBuilder.AppendFormat( CultureInfo.InvariantCulture, "{0},{1};", i, j );
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}