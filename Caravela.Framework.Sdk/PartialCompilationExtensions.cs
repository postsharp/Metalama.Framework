// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Sdk
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
            => compilation.UpdateSyntaxTrees(
                compilation.SyntaxTrees.Select( t => (Old: t, New: getNewTree( t, cancellationToken )) )
                    .Where( t => t.New != t.Old )
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
            => compilation.UpdateSyntaxTrees(
                compilation.SyntaxTrees.Select( t => (OldTree: t, NewRoot: getNewSyntaxRoot( t.GetRoot( cancellationToken ), cancellationToken )) )
                    .Where( x => x.OldTree.GetRoot( cancellationToken ) != x.NewRoot )
                    .Select( x => (x.OldTree, NewTree: x.OldTree.WithRootAndOptions( x.NewRoot, (CSharpParseOptions) x.OldTree.Options )) )
                    .ToList(),
                Array.Empty<SyntaxTree>() );
    }
}