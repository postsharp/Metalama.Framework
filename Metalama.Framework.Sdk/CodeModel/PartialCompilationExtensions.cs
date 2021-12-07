// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Impl.CodeModel
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
        /// <param name="updateTree">A function that maps the old <see cref="SyntaxTree"/> to the new <see cref="SyntaxTree"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new <see cref="IPartialCompilation"/>.</returns>
        public static IPartialCompilation UpdateSyntaxTrees(
            this IPartialCompilation compilation,
            Func<SyntaxTree, CancellationToken, SyntaxTree> updateTree,
            CancellationToken cancellationToken = default )
            => compilation.WithSyntaxTreeModifications(
                compilation.SyntaxTrees.Values.Select( t => new SyntaxTreeModification( updateTree( t, cancellationToken ), t ) )
                    .Where( t => t.NewTree != t.OldTree )
                    .ToList(),
                Array.Empty<SyntaxTree>() );

        /// <summary>
        /// Updates the syntax trees of a given <see cref="IPartialCompilation"/> by providing a function that maps
        /// a <see cref="SyntaxTree"/> to a transformed <see cref="SyntaxTree"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="updateSyntaxRoot">A function that maps the old root <see cref="SyntaxNode"/> to the new <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new <see cref="IPartialCompilation"/>.</returns>
        public static IPartialCompilation UpdateSyntaxTrees(
            this IPartialCompilation compilation,
            Func<SyntaxNode, CancellationToken, SyntaxNode> updateSyntaxRoot,
            CancellationToken cancellationToken = default )
            => compilation.WithSyntaxTreeModifications(
                compilation.SyntaxTrees.Values.Select( t => (OldTree: t, NewRoot: updateSyntaxRoot( t.GetRoot( cancellationToken ), cancellationToken )) )
                    .Where( x => x.OldTree.GetRoot( cancellationToken ) != x.NewRoot )
                    .Select( x => new SyntaxTreeModification( x.OldTree.WithRootAndOptions( x.NewRoot, (CSharpParseOptions) x.OldTree.Options ), x.OldTree ) )
                    .ToList(),
                Array.Empty<SyntaxTree>() );

        public static IPartialCompilation UpdateSyntaxTrees(
            this IPartialCompilation compilation,
            Func<SyntaxTree, SyntaxTree> updateTree,
            CancellationToken cancellationToken = default )
        {
            var modifiedSyntaxTrees = new List<SyntaxTreeModification>( compilation.SyntaxTrees.Count );

            foreach ( var tree in compilation.SyntaxTrees.Values )
            {
                var newTree = updateTree( tree );

                cancellationToken.ThrowIfCancellationRequested();

                if ( newTree != tree )
                {
                    modifiedSyntaxTrees.Add( new SyntaxTreeModification( newTree, tree ) );
                }
            }

            return compilation.WithSyntaxTreeModifications( modifiedSyntaxTrees );
        }

        public static IPartialCompilation RewriteSyntaxTrees(
            this IPartialCompilation compilation,
            CSharpSyntaxRewriter rewriter,
            CancellationToken cancellationToken = default )
        {
            var modifiedSyntaxTrees = new List<SyntaxTreeModification>( compilation.SyntaxTrees.Count );

            foreach ( var tree in compilation.SyntaxTrees.Values )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var oldRoot = tree.GetRoot();
                var newRoot = rewriter.Visit( oldRoot );

                if ( newRoot != oldRoot )
                {
                    modifiedSyntaxTrees.Add( new SyntaxTreeModification( tree.WithRootAndOptions( newRoot, tree.Options ), tree ) );
                }
            }

            return compilation.WithSyntaxTreeModifications( modifiedSyntaxTrees );
        }

        public static IPartialCompilation AddSyntaxTrees( this IPartialCompilation compilation, params SyntaxTree[] syntaxTrees )
            => compilation.WithSyntaxTreeModifications( null, syntaxTrees );

        public static IPartialCompilation AddSyntaxTrees( this IPartialCompilation compilation, IEnumerable<SyntaxTree> syntaxTrees )
            => compilation.WithSyntaxTreeModifications( null, syntaxTrees.ToList() );
    }
}