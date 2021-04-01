// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        private abstract class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ISymbolClassifier _symbolClassifier;

            protected Rewriter( ISymbolClassifier symbolClassifier, Compilation runTimeCompilation )
            {
                this._symbolClassifier = symbolClassifier;
                this.RunTimeCompilation = runTimeCompilation;
            }

            protected Compilation RunTimeCompilation { get; }

            protected static MethodDeclarationSyntax WithThrowNotSupportedExceptionBody( MethodDeclarationSyntax method, string message )
            {
                // Method does not have a body (e.g. because it's abstract) , so there is nothing to replace
                if ( method.Body == null && method.ExpressionBody == null )
                {
                    return method;
                }

                // throw new System.NotSupportedException("message")
                var body = ThrowExpression( ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                    .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );

                return method.WithBody( null ).WithExpressionBody( ArrowExpressionClause( body ) ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
            }

            protected SymbolDeclarationScope GetSymbolDeclarationScope( MemberDeclarationSyntax node )
            {
                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
                return this._symbolClassifier.GetSymbolDeclarationScope( symbol );
            }
        }
    }
}
