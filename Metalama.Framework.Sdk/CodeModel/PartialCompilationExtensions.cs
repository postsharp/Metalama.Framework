// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Extension methods for the <see cref="IPartialCompilation"/> interface.
    /// </summary>
    internal static class PartialCompilationExtensions
    {
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
            => compilation.WithSyntaxTreeTransformations(
                compilation.SyntaxTrees.Values.Select( t => (OldTree: t, NewRoot: updateSyntaxRoot( t.GetRoot( cancellationToken ), cancellationToken )) )
                    .Where( x => x.OldTree.GetRoot( cancellationToken ) != x.NewRoot )
                    .Select(
                        x => SyntaxTreeTransformation.ReplaceTree(
                            x.OldTree,
                            x.OldTree.WithRootAndOptions( x.NewRoot, (CSharpParseOptions) x.OldTree.Options ) ) )
                    .ToList() );

        public static async Task<IPartialCompilation> RewriteSyntaxTreesAsync(
            this IPartialCompilation compilation,
            CSharpSyntaxRewriter rewriter,
            IServiceProvider<IProjectService> serviceProvider,
            CancellationToken cancellationToken = default )
        {
            var taskScheduler = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
            var modifiedSyntaxTrees = new ConcurrentBag<SyntaxTreeTransformation>();

            await taskScheduler.RunInParallelAsync( compilation.SyntaxTrees.Values, RewriteSyntaxTreeAsync, cancellationToken );

            async Task RewriteSyntaxTreeAsync( SyntaxTree tree )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var oldRoot = await tree.GetRootAsync( cancellationToken );
                var newRoot = rewriter.Visit( oldRoot );

                if ( newRoot != oldRoot )
                {
                    modifiedSyntaxTrees.Add( SyntaxTreeTransformation.ReplaceTree( tree, tree.WithRootAndOptions( newRoot, tree.Options ) ) );
                }
            }

            return compilation.WithSyntaxTreeTransformations( modifiedSyntaxTrees.ToList() );
        }

        public static IPartialCompilation AddSyntaxTrees( this IPartialCompilation compilation, params SyntaxTree[] syntaxTrees )
            => compilation.WithSyntaxTreeTransformations( syntaxTrees.Select( SyntaxTreeTransformation.AddTree ).ToList() );
    }
}