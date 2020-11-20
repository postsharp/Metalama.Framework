using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    // TODO: also remove compile-time-only types
    class StripCaravelaRewriter : CSharpSyntaxRewriter
    {
        private readonly CSharpCompilation _compilation;
        private readonly bool _enabled;

        private SemanticModel _semanticModel = null!;

        public StripCaravelaRewriter( CSharpCompilation compilation, bool enabled )
        {
            this._compilation = compilation;
            this._enabled = enabled;
        }

        public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
        {
            if ( this._enabled )
                this._semanticModel = this._compilation.GetSemanticModel( node.SyntaxTree );

            var result = base.VisitCompilationUnit( node );

            this._semanticModel = null!;

            return result;
        }

        public override SyntaxNode? VisitAttributeList( AttributeListSyntax node )
        {
            var newNode = (AttributeListSyntax) base.VisitAttributeList( node )!;

            if ( !newNode.Attributes.Any() )
                return null;

            return newNode;
        }

        public override SyntaxNode? VisitAttribute( AttributeSyntax node )
        {
            if ( this._enabled )
            {
                var attributeSymbol = this._semanticModel.GetSymbolInfo( node );

                var attributeType = ((IMethodSymbol) attributeSymbol.Symbol!).ContainingType;

                if ( IsCaravelaType( attributeType ) )
                    return null;
            }

            return node;
        }

        private static bool IsCaravelaType( ITypeSymbol typeSymbol )
        {
            if ( typeSymbol.ContainingAssembly.Name.StartsWith( "Caravela.Framework", StringComparison.Ordinal ) )
                return true;

            if ( typeSymbol.BaseType != null )
            {
                if ( IsCaravelaType( typeSymbol.BaseType ) )
                    return true;
            }

            foreach ( var iface in typeSymbol.Interfaces )
            {
                if ( IsCaravelaType( iface ) )
                    return true;
            }

            return false;
        }
    }
}
