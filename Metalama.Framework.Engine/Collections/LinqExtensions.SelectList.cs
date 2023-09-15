// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections;
using System.Collections.Generic;

namespace System.Linq;

public static partial class LinqExtensions
{
    private class SelectList<TIn, TOut> : IReadOnlyList<TOut>
    {
        private readonly IReadOnlyList<TIn> _input;
        private readonly Func<TIn, TOut> _func;

        public SelectList( IReadOnlyList<TIn> input, Func<TIn, TOut> func )
        {
            this._input = input;
            this._func = func;
        }

        public IEnumerator<TOut> GetEnumerator()
        {
            for ( var i = 0; i < this.Count; i++ )
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._input.Count;

        public TOut this[ int index ] => this._func( this._input[index] );
    }
}