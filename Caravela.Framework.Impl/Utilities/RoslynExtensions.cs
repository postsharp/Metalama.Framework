// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

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
            where TCompilation : Compilation
            => compilation.ReplaceAllTrees(
                tree =>
                {
                    var newRoot = rewriter.Visit( tree.GetRoot() );

                    return newRoot == null ? null : tree.WithRootAndOptions( newRoot, tree.Options );
                } );
    }
}