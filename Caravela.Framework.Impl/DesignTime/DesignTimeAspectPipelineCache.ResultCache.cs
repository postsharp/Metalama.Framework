// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{

    internal static partial class DesignTimeAspectPipelineCache
    {

        /// <summary>
        /// Caches the pipeline results for each syntax tree.
        /// </summary>
        private static class SyntaxTreeResultCache
        {
            /// <summary>
            /// Maps the syntax tree name to the pipeline result for this syntax tree.
            /// </summary>
            private static readonly ConcurrentDictionary<string, SyntaxTreeResult> _syntaxTreeCache = new();

            /// <summary>
            /// Updates cache with a <see cref="DesignTimeAspectPipelineResult"/> that includes results for several syntax trees.
            /// </summary>
            /// <param name="results"></param>
            public static void Update( Compilation compilation, DesignTimeAspectPipelineResult results )
            {
                var resultsByTree = SplitResultsByTree( compilation, results );

                foreach ( var result in resultsByTree )
                {
                    _syntaxTreeCache[result.SyntaxTree.FilePath] = result;
                }
            }

            /// <summary>
            /// Splits a <see cref="DesignTimeAspectPipelineResult"/>, which includes data for several syntax trees, into
            /// a list of <see cref="SyntaxTreeResult"/> which each have information related to a single syntax tree.
            /// </summary>
            /// <param name="results"></param>
            /// <returns></returns>
            private static IEnumerable<SyntaxTreeResult> SplitResultsByTree( Compilation compilation, DesignTimeAspectPipelineResult results )
            {
                var resultsByTree = results
                    .InputSyntaxTrees
                    .ToDictionary( r => r.FilePath, syntaxTree => new SyntaxTreeResultBuilder( syntaxTree ) );

                // Split diagnostic by syntax tree.
                foreach ( var diagnostic in results.Diagnostics.ReportedDiagnostics )
                {
                    var filePath = diagnostic.Location.SourceTree?.FilePath;

                    if ( filePath != null )
                    {
                        if ( resultsByTree.TryGetValue( filePath, out var builder ) )
                        {
                            builder.Diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
                            builder.Diagnostics.Add( diagnostic );
                        }
                        else
                        {
                            // This can happen when a CS error is reported in the aspect. These errors can be ignored.
                        }
                    }
                }

                // Split suppressions by syntax tree.
                foreach ( var suppression in results.Diagnostics.DiagnosticSuppressions )
                {
                    void AddSuppression( string? path )
                    {
                        if ( !string.IsNullOrEmpty( path ) )
                        {
                            var builder = resultsByTree[path!];
                            builder.Suppressions ??= ImmutableArray.CreateBuilder<CacheableScopedSuppression>();
                            builder.Suppressions.Add( new CacheableScopedSuppression( suppression ) );
                        }
                    }

                    var declaringSyntaxes = ((ICodeElementInternal) suppression.CodeElement).DeclaringSyntaxReferences;

                    switch ( declaringSyntaxes.Length )
                    {
                        case 0:
                            continue;

                        case 1:
                            AddSuppression( declaringSyntaxes[0].SyntaxTree.FilePath );

                            break;

                        default:
                            foreach ( var filePath in declaringSyntaxes.Select( p => p.SyntaxTree.FilePath ).Distinct() )
                            {
                                AddSuppression( filePath );
                            }

                            break;
                    }
                }

                // Split introductions by original syntax tree.
                foreach ( var introduction in results.IntroducedSyntaxTrees )
                {
                    var filePath = introduction.SourceSyntaxTree.FilePath;
                    var builder = resultsByTree[filePath];
                    builder.Introductions ??= ImmutableArray.CreateBuilder<IntroducedSyntaxTree>();
                    builder.Introductions.Add( introduction );
                }

                return resultsByTree.Select( b => b.Value.ToImmutable(compilation) );
            }

            public static bool TryGetValue( SyntaxTree syntaxTree, [NotNullWhen( true )] out SyntaxTreeResult? result, bool validateDependencies )
            {
                if ( !_syntaxTreeCache.TryGetValue( syntaxTree.FilePath, out result ) )
                {
                    return false;
                }
                else
                {
                    if ( validateDependencies )
                    {
                        // The item is in the cache, but we need to check that all dependencies are valid.
                        foreach ( var dependency in result.Dependencies )
                        {
                            if ( !_syntaxTreeCache.ContainsKey( dependency ) )
                            {
                                // The dependency is not present, which means that it has been invalidated.
                                // Remove also the current item.
                                _ = _syntaxTreeCache.TryRemove( syntaxTree.FilePath, out _ );
                            }
                        }
                        
                        // TODO: Implement a better cache invalidation algorithm so that cache validation is less expensive.
                    }

                    return true;
                }
            }

            public static void OnSyntaxTreePossiblyChanged( SyntaxTree syntaxTree )
            {
                if ( _syntaxTreeCache.TryGetValue( syntaxTree.FilePath, out var cachedResult ) )
                {
                    // Check if the source text has changed.
                    if ( syntaxTree != cachedResult.SyntaxTree )
                    {
                        if ( !syntaxTree.GetText().ContentEquals( cachedResult.SyntaxTree.GetText() ) )
                        {
                            // If the source text has changed, check whether the change can possibly change symbols. Changes in method implementations are ignored.
                            var changedSyntaxNodes =
                                syntaxTree.GetChangedSpans( cachedResult.SyntaxTree ).Select( span => syntaxTree.GetRoot().FindNode( span ) );

                            if ( changedSyntaxNodes.Any( x => !IsIrrelevantChange(x) ) )
                            {
                                _ = _syntaxTreeCache.TryRemove( cachedResult.SyntaxTree.FilePath, out _ );

                                return;
                            }
                        }

                        // Update the syntax tree so the next comparison is less demanding.
                        cachedResult.LastComparedSyntaxTree = syntaxTree;
                    }
                }


                // Determines if a change in a node can possibly affect a change in symbols.
                static bool IsIrrelevantChange( SyntaxNode node )
                    => node.Parent switch
                    {
                        BaseMethodDeclarationSyntax method => node == method.Body || node == method.ExpressionBody,
                        AccessorDeclarationSyntax accessor => node == accessor.Body || node == accessor.ExpressionBody,
                        VariableDeclaratorSyntax field => node == field.Initializer?.Value,
                        _ => node.Parent != null && IsIrrelevantChange( node.Parent )
                    };
            }

            public static void Clear()
            {
                _syntaxTreeCache.Clear();
            }
        }
    }
}