using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    internal class ProceedCallAnnotator : CSharpSyntaxRewriter
    {
        private int calls;
        private readonly SemanticAnnotationMap _semanticAnnotationMap;

        public ProceedCallAnnotator( SemanticAnnotationMap semanticAnnotationMap )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
        }
        
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

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
                if ( this.calls > 0 )
                {
                    this.Diagnostics.Add( Diagnostic.Create( "CA07", "Annotation",
                        $"The {node} method cannot be called twice in a template method.",
                        DiagnosticSeverity.Error,
                        DiagnosticSeverity.Error, true, 0, location: Location.Create( node.SyntaxTree, node.Span ) ) );

                }

                this.calls++;
                return node.AddCallsProceedAnnotation();
            }
            else
            {
                return base.VisitInvocationExpression( node );
            }
        }
    }
}