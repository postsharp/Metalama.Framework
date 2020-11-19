using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// Has <see cref="CreateCommaSeparatedList"/>.
    /// </summary>
    public static class InitializerFormer
    {
        /// <summary>
        /// Creates a comma-separated list of expressions.
        /// </summary>
        /// <param name="elements">Expressions to be separated by a comma token.</param>
        public static SeparatedSyntaxList<ExpressionSyntax> CreateCommaSeparatedList( IEnumerable<ExpressionSyntax> elements )
        {
            List<SyntaxNodeOrToken> lt = new List<SyntaxNodeOrToken>();
            bool first = true;
            foreach ( var obj in elements)
            {
                if ( !first )
                {
                    lt.Add( SyntaxFactory.Token( SyntaxKind.CommaToken ) );
                }
                lt.Add( obj );
                first = false;
            }
            var list = SyntaxFactory.SeparatedList<ExpressionSyntax>( lt );
            return list;
        } 
    }
}