using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Caravela.AspectWorkbench
{
    public static class SyntaxAnnotationExtensions
    {
        private static readonly SyntaxAnnotation buildTimeOnlyAnnotation = new SyntaxAnnotation("scope", "buildtime");
        private static readonly SyntaxAnnotation runTimeOnlyAnnotation = new SyntaxAnnotation("scope", "runtime");
        private static readonly SyntaxAnnotation noDeepIndentAnnotation = new SyntaxAnnotation("noindent");
        
        public static bool HasScopeAnnotation(this SyntaxNode node)
        {
            return node.HasAnnotations("scope");
        }
        
        public static SymbolScope GetScopeFromAnnotation(this SyntaxNode node)
        {
            var annotation = node.GetAnnotations("scope").SingleOrDefault();
            if (annotation == null)
            {
                return SymbolScope.Default;
            }
            else
            {
                switch (annotation.Data)
                {
                    case "buildtime":
                        return SymbolScope.CompileTimeOnly;
                    
                    case "runtime":
                        return SymbolScope.RunTimeOnly;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        public static T AddScopeAnnotation<T>(this T node, SymbolScope scope) where T : SyntaxNode
        {
            if (scope == SymbolScope.Default)
            {
                return node;
            }
            
            var existingScope = node.GetScopeFromAnnotation();

            if (existingScope != SymbolScope.Default)
            {
                if (existingScope == scope)
                {
                    return node;
                }
                else if (existingScope != scope)
                {
                    throw new Exception();
                }
            }

            switch (scope)
            {
                case SymbolScope.CompileTimeOnly:
                    return node.WithAdditionalAnnotations(buildTimeOnlyAnnotation);
                
                case SymbolScope.RunTimeOnly:
                    return node.WithAdditionalAnnotations(runTimeOnlyAnnotation);
                
                default:
                    return node;
            }
        }

        public static T WithScopeAnnotationFrom<T>(this T node, SyntaxNode source) where T : SyntaxNode
            => node.AddScopeAnnotation(source.GetScopeFromAnnotation());


        public static T WithSymbolAnnotationsFrom<T>(this T node, SyntaxNode source) where T : SyntaxNode => 
            node.WithAdditionalAnnotations(source.GetAnnotations(SemanticAnnotationMap.AnnotationKinds));

        public static T AddNoDeepIndentAnnotation<T>(this T node) where T : SyntaxNode =>
            node.WithAdditionalAnnotations(noDeepIndentAnnotation);
        
        public static bool HasNoDeepIndentAnnotation(this SyntaxNode node) => node.HasAnnotation(noDeepIndentAnnotation);

        
    }
}