using System.Linq;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        private class PrepareRunTimeAssemblyRewriter : Rewriter
        {
            private readonly INamedTypeSymbol? _aspectDriverSymbol;

            public PrepareRunTimeAssemblyRewriter( ISymbolClassifier symbolClassifier, Compilation compilation )
                : base( symbolClassifier, compilation )
            {
                this._aspectDriverSymbol = compilation.GetTypeByMetadataName( typeof( IAspectDriver ).FullName );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var symbol = this.Compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

                // Special case: aspect weavers and other aspect drivers are preserved in the runtime assembly.
                // This only happens if regular Caravela.Framework is referenced from the weaver project, which generally shouldn't happen.
                // But it is a pattern used by Caravela.Samples for try.postsharp.net.
                if ( this._aspectDriverSymbol != null && symbol.AllInterfaces.Any( i => SymbolEqualityComparer.Default.Equals( i, this._aspectDriverSymbol ) ) )
                {
                    return node;
                }

                return base.VisitClassDeclaration( node );
            }

            public override SyntaxNode VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) is SymbolDeclarationScope.CompileTimeOnly or SymbolDeclarationScope.Template )
                {
                    return WithThrowNotSupportedExceptionBody( node, "Compile-time only code cannot be called at run-time." );
                }

                return node;
            }
        }
    }
}
