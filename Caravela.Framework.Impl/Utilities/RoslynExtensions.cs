using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Utilities
{
    public static class RoslynExtensions
    {

        public static TCompilation ReplaceAllTrees<TCompilation>( this TCompilation compilation, Func<SyntaxTree, SyntaxTree?> replacer )
            where TCompilation : Compilation
        {
            foreach ( var tree in compilation.SyntaxTrees )
            {
                var newTree = replacer( tree );
                if ( newTree != null )
                {
                    compilation = (TCompilation) compilation.ReplaceSyntaxTree( tree, newTree );
            }
                else
                {
                    compilation = (TCompilation) compilation.RemoveSyntaxTrees( tree );
                }
            }

            return compilation;
        }

        public static TCompilation VisitAllTrees<TCompilation>( this CSharpSyntaxRewriter rewriter, TCompilation compilation )
            where TCompilation : Compilation =>
            compilation.ReplaceAllTrees( tree => tree.WithRootAndOptions( rewriter.Visit( tree.GetRoot() ), tree.Options ) );
    }
}
