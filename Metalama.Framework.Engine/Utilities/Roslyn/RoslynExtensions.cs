// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    /// <summary>
    /// Provides extension methods that are useful when writing code using Metalama SDK.
    /// </summary>
    public static class RoslynExtensions
    {
        public static TCompilation ReplaceTrees<TCompilation>(
            this TCompilation compilation,
            Func<SyntaxTree, SyntaxTree?> replacer,
            IEnumerable<SyntaxTree>? trees = null )
            where TCompilation : Compilation
        {
            trees ??= compilation.SyntaxTrees;

            foreach ( var tree in trees )
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

        public static TCompilation VisitTrees<TCompilation>(
            this CSharpSyntaxRewriter rewriter,
            TCompilation compilation,
            IEnumerable<SyntaxTree>? trees = null )
            where TCompilation : Compilation
            => compilation.ReplaceTrees(
                tree =>
                {
                    var newRoot = rewriter.Visit( tree.GetRoot() );

                    return newRoot == null ? null : tree.WithRootAndOptions( newRoot, tree.Options );
                },
                trees );
    }
}