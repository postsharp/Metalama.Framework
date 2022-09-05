// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Allows to build a run-time expression by composing a string thanks to an underlying <see cref="System.Text.StringBuilder"/>.
    /// Use the <see cref="ToExpression"/> method to convert the <see cref="ExpressionBuilder"/> into a compile-time representation of the expression,
    /// or the <see cref="ExpressionBuilderExtensions.ToValue(Metalama.Framework.Code.SyntaxBuilders.IExpressionBuilder)"/> methods converts it to a dynamic expression that can be used in the C# code
    /// of the template. 
    /// </summary>
    [CompileTime]
    public sealed class ExpressionBuilder : SyntaxBuilder, IExpressionBuilder
    {
        public ExpressionBuilder() { }

        private ExpressionBuilder( ExpressionBuilder prototype ) : base( prototype ) { }

        /// <summary>
        /// Creates a compile-time <see cref="IExpression"/> from the current <see cref="ExpressionBuilder"/>.
        /// </summary>
        public IExpression ToExpression() => ExpressionFactory.Parse( this.StringBuilder.ToString() );

        /// <summary>
        /// Returns a clone of the current <see cref="ExpressionBuilder"/>.
        /// </summary>
        public ExpressionBuilder Clone() => new( this );
    }
}