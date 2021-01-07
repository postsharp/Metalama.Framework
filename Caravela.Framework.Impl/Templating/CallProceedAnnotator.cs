using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    internal class CallProceedAnnotator : CSharpSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;

        public CallProceedAnnotator( SemanticAnnotationMap semanticAnnotationMap )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            
            var transformedNode = base.Visit( node );

            if ( transformedNode == null )
            {
                return null;
            }
            
            if ( transformedNode.ChildNodes().Any( n => n.HasCallsProceedAnnotation() ) )
            {
                return transformedNode.AddCallsProceedAnnotation();
            }
            else
            {
                return transformedNode;
            }
        } 
        
        private bool IsProceed(SyntaxNode node)
        {
            var symbol = this._semanticAnnotationMap.GetSymbol(node);
            
            if (symbol == null)
            {
                return false;
            }

            return symbol.GetAttributes().Any(a => a.AttributeClass.Name == nameof(ProceedAttribute));
        }

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            if ( this.IsProceed( node.Expression ) )
            {
                return node.AddCallsProceedAnnotation();
            }
            else
            {
                return base.VisitInvocationExpression( node );
            }
        }
    }
}