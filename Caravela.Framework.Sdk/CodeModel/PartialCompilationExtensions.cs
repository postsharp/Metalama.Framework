// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Extension methods for the <see cref="IPartialCompilation"/> interface.
    /// </summary>
    public static class PartialCompilationExtensions
    {
        /// <summary>
        /// Updates the syntax trees of a given <see cref="IPartialCompilation"/> by providing a function that maps
        /// a <see cref="SyntaxTree"/> to a transformed <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="getNewTree">A function that maps the old <see cref="SyntaxTree"/> to the new <see cref="SyntaxTree"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new <see cref="IPartialCompilation"/>.</returns>
        public static IPartialCompilation UpdateSyntaxTrees(
            this IPartialCompilation compilation,
            Func<SyntaxTree, CancellationToken, SyntaxTree> getNewTree,
            CancellationToken cancellationToken = default )
            => compilation.WithSyntaxTrees(
                compilation.SyntaxTrees.Values.Select( t => new ModifiedSyntaxTree( getNewTree( t, cancellationToken ), t ) )
                    .Where( t => t.NewTree != t.OldTree )
                    .ToList(),
                Array.Empty<SyntaxTree>() );

        /// <summary>
        /// Updates the syntax trees of a given <see cref="IPartialCompilation"/> by providing a function that maps
        /// a <see cref="SyntaxTree"/> to a transformed <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="getNewSyntaxRoot">A function that maps the old root <see cref="SyntaxNode"/> to the new <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new <see cref="IPartialCompilation"/>.</returns>
        public static IPartialCompilation UpdateSyntaxTrees(
            this IPartialCompilation compilation,
            Func<SyntaxNode, CancellationToken, SyntaxNode> getNewSyntaxRoot,
            CancellationToken cancellationToken = default )
            => compilation.WithSyntaxTrees(
                compilation.SyntaxTrees.Values.Select( t => (OldTree: t, NewRoot: getNewSyntaxRoot( t.GetRoot( cancellationToken ), cancellationToken )) )
                    .Where( x => x.OldTree.GetRoot( cancellationToken ) != x.NewRoot )
                    .Select( x => new ModifiedSyntaxTree( x.OldTree.WithRootAndOptions( x.NewRoot, (CSharpParseOptions) x.OldTree.Options ), x.OldTree ) )
                    .ToList(),
                Array.Empty<SyntaxTree>() );
        
        
        public static IPartialCompilation UpdateSyntaxTrees( this IPartialCompilation compilation, Func<SyntaxTree, SyntaxTree> replace, CancellationToken cancellationToken = default )
        {
            var modifiedSyntaxTrees = new List<ModifiedSyntaxTree>( compilation.SyntaxTrees.Count );
            
            foreach ( var tree in compilation.SyntaxTrees.Values )
            {
                var newTree = replace( tree );
                
                cancellationToken.ThrowIfCancellationRequested();

                if ( newTree != tree )
                {
                    modifiedSyntaxTrees.Add( new ModifiedSyntaxTree(newTree,tree) );
                }
            }

            return compilation.WithSyntaxTrees( modifiedSyntaxTrees );
        }

        public static IPartialCompilation RewriteSyntaxTrees( this IPartialCompilation compilation, CSharpSyntaxRewriter rewriter, CancellationToken cancellationToken = default )
        {
            var modifiedSyntaxTrees = new List<ModifiedSyntaxTree>( compilation.SyntaxTrees.Count );
            
            foreach ( var tree in compilation.SyntaxTrees.Values )
            {
                cancellationToken.ThrowIfCancellationRequested();
                    
                var oldRoot = tree.GetRoot();
                var newRoot = rewriter.Visit( oldRoot );

                if ( newRoot != oldRoot )
                {
                    modifiedSyntaxTrees.Add( new ModifiedSyntaxTree(tree.WithRootAndOptions( newRoot, tree.Options ), tree ) );
                }
            }

            return compilation.WithSyntaxTrees( modifiedSyntaxTrees );
        }
    }
}