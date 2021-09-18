// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Allows to build a run-time expression by composing a string thanks to an underlying <see cref="System.Text.StringBuilder"/>.
    /// Use the <see cref="ToExpression"/> method to convert the <see cref="ExpressionBuilder"/> into a compile-time representation of the expression,
    /// or the <see cref="ExpressionBuilderExtensions.ToValue"/> methods converts it to a dynamic expression that can be used in the C# code
    /// of the template. 
    /// </summary>
    [CompileTimeOnly]
    public sealed class ExpressionBuilder : SyntaxBuilder, IExpressionBuilder
    {
        public ExpressionBuilder() { }

        private ExpressionBuilder( ExpressionBuilder prototype ) : base( prototype ) { }

        /// <summary>
        /// Creates a compile-time <see cref="IExpression"/> from the current <see cref="ExpressionBuilder"/>.
        /// </summary>
        public IExpression ToExpression() => meta.ParseExpression( this.StringBuilder.ToString() );

        /// <summary>
        /// Returns a clone of the current <see cref="ExpressionBuilder"/>.
        /// </summary>
        /// <returns></returns>
        public ExpressionBuilder Clone() => new( this );
    }
}