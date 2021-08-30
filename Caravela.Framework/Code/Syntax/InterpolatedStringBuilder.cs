// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Syntax
{
    /// <summary>
    /// Compile-time object that allows to build a run-time interpolated string.
    /// </summary>
    [CompileTimeOnly]
    public sealed class InterpolatedStringBuilder : ISyntaxBuilder
    {
        private readonly List<object?> _items = new();

        internal IReadOnlyList<object?> Items => this._items;

        private InterpolatedStringBuilder() { }

        private InterpolatedStringBuilder( InterpolatedStringBuilder prototype )
        {
            this._items.AddRange( prototype._items );
        }

        /// <summary>
        /// Creates a new <see cref="InterpolatedStringBuilder"/>.
        /// </summary>
        public static InterpolatedStringBuilder Create() => new();

        /// <summary>
        /// Adds a fixed text to the interpolated string.
        /// </summary>
        /// <param name="text"></param>
        public void AddText( string text ) => this._items.Add( text );

        /// <summary>
        /// Adds an expression to the interpolated string.
        /// </summary>
        /// <param name="expression"></param>
        public void AddExpression( dynamic? expression ) => this._items.Add( new Token( expression ) );

        /// <summary>
        /// Converts the current <see cref="InterpolatedStringBuilder"/> to syntax that represents the interpolated string.
        /// </summary>
        /// <returns></returns>
        public dynamic ToInterpolatedString() => meta.CurrentContext.CodeBuilder.BuildInterpolatedString( this );

        internal class Token
        {
            public object? Expression { get; }

            public Token( object? expression )
            {
                this.Expression = expression;
            }
        }

        ISyntax ISyntaxBuilder.ToSyntax() => this.ToInterpolatedString();

        public InterpolatedStringBuilder Clone() => new( this );
    }
}