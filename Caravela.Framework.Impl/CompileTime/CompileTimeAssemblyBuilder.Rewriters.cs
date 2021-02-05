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

            protected Rewriter( ISymbolClassifier symbolClassifier, Compilation compilation )
            {
                this._symbolClassifier = symbolClassifier;
                this.Compilation = compilation;
            }

            protected Compilation Compilation { get; }

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

                return method.WithBody( null ).WithExpressionBody( ArrowExpressionClause( body ) );
            }

            protected SymbolDeclarationScope GetSymbolDeclarationScope( MemberDeclarationSyntax node )
            {
                var symbol = this.Compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
                return this._symbolClassifier.GetSymbolDeclarationScope( symbol );
            }
        }
    }
}
