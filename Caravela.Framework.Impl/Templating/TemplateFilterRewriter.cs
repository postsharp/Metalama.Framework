using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PostSharp.Caravela.AspectWorkbench
{
    /// <summary>
    /// A forwarding <see cref="CSharpSyntaxRewriter"/> that only forwards
    /// 'interesting' declarations and ignores the other ones. This should be generalized
    /// into something that filters build-time expressions. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TemplateRewriterFilter<T> : CSharpSyntaxRewriter
        where T : CSharpSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;
        public T Inner { get; }

        public TemplateRewriterFilter(SemanticAnnotationMap semanticAnnotationMap, T inner)
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
            this.Inner = inner;
        }

        private bool IsTemplate(SyntaxNode node)
        {
            var symbol = this._semanticAnnotationMap.GetDeclaredSymbol(node);
            if (symbol != null)
            {
                return this.IsTemplate(symbol);
            }
            else
            {
                return false;
            }
        }
        
        private bool IsTemplate(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass.Name == nameof(TemplateAttribute));
        }
        
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return this.IsTemplate(node) ? this.Inner.VisitMethodDeclaration(node) : node;
        }
        

        public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return this.IsTemplate(node) ? this.Inner.VisitPropertyDeclaration(node) : node;
        }

        public override SyntaxNode? VisitEventDeclaration(EventDeclarationSyntax node)
        {
            return this.IsTemplate(node) ? this.Inner.VisitEventDeclaration(node) : node;
        }
        
    }
}