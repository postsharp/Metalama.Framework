using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    public static class RoslynExtensions
    {
        public static TCompilation VisitAllTrees<TCompilation>( this CSharpSyntaxRewriter rewriter, TCompilation compilation )
            where TCompilation : Compilation
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                compilation = (TCompilation) compilation.ReplaceSyntaxTree( tree, tree.WithRootAndOptions( rewriter.Visit( tree.GetRoot() ), tree.Options ) );
            }

            return compilation;
        }
    }
}
