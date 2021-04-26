// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
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
                        var builder = resultsByTree[filePath];
                        builder.Diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
                        builder.Diagnostics.Add( diagnostic );
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
                            builder.Suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
                            builder.Suppressions.Add( suppression );
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
                                _syntaxTreeCache.TryRemove( syntaxTree.FilePath, out _ );
                            }
                        }
                    }

                    return true;
                }
            }

            public static void OnSyntaxTreeUpdated( SyntaxTree syntaxTree )
            {
                if ( _syntaxTreeCache.TryGetValue( syntaxTree.FilePath, out var cachedResult ) )
                {
                    if ( !syntaxTree.GetText().ContentEquals( cachedResult.SyntaxTree.GetText() ) )
                    {
                        _ = _syntaxTreeCache.TryRemove( cachedResult.SyntaxTree.FilePath, out _ );
                    }
                }
            }
        }
    }
}