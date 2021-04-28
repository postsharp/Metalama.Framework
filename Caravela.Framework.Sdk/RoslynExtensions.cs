// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Sdk
{
    public static class RoslynExtensions
    {
        public static bool AnyBaseType( this INamedTypeSymbol type, Predicate<INamedTypeSymbol> predicate )
        {
            for ( var t = type; t != null; t = t.BaseType )
            {
                if ( predicate( t ) )
                {
                    return true;
                }
            }

            return false;
        }

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

        public static IEnumerable<INamedTypeSymbol> GetTypes( this IAssemblySymbol assembly ) => assembly.GlobalNamespace.GetTypes();

        private static IEnumerable<INamedTypeSymbol> GetTypes( this INamespaceSymbol ns )
        {
            foreach ( var type in ns.GetTypeMembers() )
            {
                yield return type;
            }

            foreach ( var namespaceMember in ns.GetNamespaceMembers() )
            {
                foreach ( var type in namespaceMember.GetTypes() )
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<INamedTypeSymbol> OrderByInheritance( this IReadOnlyList<INamedTypeSymbol> types )
        {
            var dictionary = types.ToDictionary( t => t, _ => false );
            List<INamedTypeSymbol> result = new();

            void VisitType( INamedTypeSymbol type )
            {
                if ( !dictionary.TryGetValue( type, out var visited ) )
                {
                    // The type is not in the input.
                }
                else if ( visited )
                {
                    // There is nothing else to do.
                }
                else
                {
                    if ( type.BaseType != null )
                    {
                        VisitType( type.BaseType );
                    }

                    result.Add( type );
                    dictionary[type] = true;
                }
            }

            foreach ( var type in types )
            {
                VisitType( type );
            }

            return result;
        }

        public static object? GetValueSafe( this TypedConstant typedConstant )
            => typedConstant.Kind == TypedConstantKind.Array ? typedConstant.Values : typedConstant.Value;
    }
}