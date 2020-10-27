using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    public static class RoslynExtensions
    {
        public static CSharpCompilation VisitAllTrees( this CSharpSyntaxRewriter rewriter, CSharpCompilation compilation )
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                compilation = compilation.ReplaceSyntaxTree( tree, tree.WithRootAndOptions( rewriter.Visit( tree.GetRoot() ), tree.Options ) );
            }

            return compilation;
        }
    }
}
