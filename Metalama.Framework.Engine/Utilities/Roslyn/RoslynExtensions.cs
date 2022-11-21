// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        public static string ToDisplayName( this SymbolKind symbolKind )
            => symbolKind switch
            {
                SymbolKind.Alias => "alias",
                SymbolKind.ArrayType => "array type",
                SymbolKind.Assembly => "assembly",
                SymbolKind.DynamicType => "dynamic type",
                SymbolKind.ErrorType => "invalid type",
                SymbolKind.Event => "event",
                SymbolKind.Field => "field",
                SymbolKind.Label => "label",
                SymbolKind.Local => "local variable",
                SymbolKind.Method => "method",
                SymbolKind.NetModule => "module",
                SymbolKind.NamedType => "type",
                SymbolKind.Namespace => "namespace",
                SymbolKind.Parameter => "parameter",
                SymbolKind.PointerType => "pointer type",
                SymbolKind.Property => "property",
                SymbolKind.RangeVariable => "range variable",
                SymbolKind.TypeParameter => "type parameter",
                SymbolKind.Preprocessing => "preprocessing directive",
                SymbolKind.Discard => "discard",
                SymbolKind.FunctionPointerType => "function pointer type",
                _ => throw new ArgumentOutOfRangeException( nameof(symbolKind), symbolKind, null )
            };
    }
}