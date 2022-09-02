// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// Adds implementation methods to the public <see cref="IExpression"/> interface. 
    /// </summary>
    public interface IUserExpression : IExpression
    {
        /// <summary>
        /// Creates a <see cref="TypedExpressionSyntax"/> for the current <see cref="TemplateExpansionContext"/>.
        /// </summary>
        /// <param name="syntaxGenerationContext"></param>
        TypedExpressionSyntax ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext );
    }

    internal static class UserExpressionExtensions
    {
        public static ExpressionSyntax ToExpressionSyntax( this IUserExpression userExpression, SyntaxGenerationContext context )
            => userExpression.ToTypedExpressionSyntax( context ).Syntax;
    }
}