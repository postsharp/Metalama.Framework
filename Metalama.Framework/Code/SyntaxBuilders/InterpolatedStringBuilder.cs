// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Compile-time object that allows to build a run-time interpolated string.
    /// </summary>
    [CompileTimeOnly]
    public sealed partial class InterpolatedStringBuilder : INotNullExpressionBuilder
    {
        private readonly List<object?> _items = new();

        internal IReadOnlyList<object?> Items => this._items;

        public InterpolatedStringBuilder() { }

        private InterpolatedStringBuilder( InterpolatedStringBuilder prototype )
        {
            this._items.AddRange( prototype._items );
        }

        /// <summary>
        /// Adds a fixed text to the interpolated string.
        /// </summary>
        /// <param name="text"></param>
        public void AddText( string? text )
        {
            if ( !string.IsNullOrEmpty( text ) )
            {
                this._items.Add( text );
            }
        }

        /// <summary>
        /// Adds an expression to the interpolated string.
        /// </summary>
        /// <param name="expression"></param>
        public void AddExpression( dynamic? expression ) => this._items.Add( new Token( expression ) );

        /// <summary>
        /// Creates a compile-time <see cref="IExpression"/> from the current <see cref="ExpressionBuilder"/>.
        /// </summary>
        public IExpression ToExpression() => SyntaxBuilder.CurrentImplementation.BuildInterpolatedString( this );

        public InterpolatedStringBuilder Clone() => new( this );
    }
}