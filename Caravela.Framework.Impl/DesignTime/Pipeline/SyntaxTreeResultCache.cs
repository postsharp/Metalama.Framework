// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the pipeline results for each syntax tree.
    /// </summary>
    internal sealed class SyntaxTreeResultCache
    {
        /// <summary>
        /// Maps the syntax tree name to the pipeline result for this syntax tree.
        /// </summary>
        private readonly ConcurrentDictionary<string, SyntaxTreeResult> _syntaxTreeCache = new();

        public int Count => this._syntaxTreeCache.Count;

        /// <summary>
        /// Updates cache with a <see cref="DesignTimeAspectPipelineResult"/> that includes results for several syntax trees.
        /// </summary>
        public void SetResults( PartialCompilation compilation, DesignTimeAspectPipelineResult results )
        {
            var resultsByTree = SplitResultsByTree( compilation, results );

            foreach ( var result in resultsByTree )
            {
                this._syntaxTreeCache[result.SyntaxTree.FilePath] = result;
            }
        }

        /// <summary>
        /// Splits a <see cref="DesignTimeAspectPipelineResult"/>, which includes data for several syntax trees, into
        /// a list of <see cref="SyntaxTreeResult"/> which each have information related to a single syntax tree.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<SyntaxTreeResult> SplitResultsByTree( PartialCompilation compilation, DesignTimeAspectPipelineResult results )
        {
            var resultBuilders = results
                .InputSyntaxTrees
                .ToDictionary( r => r.Key, syntaxTree => new SyntaxTreeResultBuilder( syntaxTree.Value ) );

            // Split diagnostic by syntax tree.
            foreach ( var diagnostic in results.Diagnostics.ReportedDiagnostics )
            {
                var filePath = diagnostic.Location.SourceTree?.FilePath;

                if ( filePath != null )
                {
                    if ( resultBuilders.TryGetValue( filePath, out var builder ) )
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
                        var builder = resultBuilders[path!];
                        builder.Suppressions ??= ImmutableArray.CreateBuilder<CacheableScopedSuppression>();
                        builder.Suppressions.Add( new CacheableScopedSuppression( suppression ) );
                    }
                }

                var declaringSyntaxes = ((IDeclarationInternal) suppression.Declaration).DeclaringSyntaxReferences;

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
                var builder = resultBuilders[filePath];
                builder.Introductions ??= ImmutableArray.CreateBuilder<IntroducedSyntaxTree>();
                builder.Introductions.Add( introduction );
            }

            // Add syntax trees with empty output to it gets cached too.
            var inputTreesWithoutOutput = compilation.SyntaxTrees.ToBuilder();

            foreach ( var path in resultBuilders.Keys )
            {
                inputTreesWithoutOutput.Remove( path );
            }

            foreach ( var empty in inputTreesWithoutOutput )
            {
                resultBuilders.Add( empty.Key, new SyntaxTreeResultBuilder( empty.Value ) );
            }

            // Return an immutable copy.
            return resultBuilders.Select( b => b.Value.ToImmutable( compilation.Compilation ) );
        }

        public bool TryGetResult( SyntaxTree syntaxTree, [NotNullWhen( true )] out SyntaxTreeResult? result )
        {
            return this._syntaxTreeCache.TryGetValue( syntaxTree.FilePath, out result );
        }

        public void InvalidateCache( CompilationChange compilationChange )
        {
            if ( !compilationChange.HasChange )
            {
                // Nothing to do.
            }
            else if ( compilationChange.HasCompileTimeCodeChange )
            {
                this._syntaxTreeCache.Clear();
            }
            else
            {
                foreach ( var change in compilationChange.SyntaxTreeChanges )
                {
                    switch ( change.SyntaxTreeChangeKind )
                    {
                        case SyntaxTreeChangeKind.Added:
                            break;

                        case SyntaxTreeChangeKind.Deleted:
                        case SyntaxTreeChangeKind.Changed:
                            Logger.Instance?.Write( $"DesignTimeSyntaxTreeResultCache.InvalidateCache({change.FilePath}): removed from cache." );
                            this._syntaxTreeCache.TryRemove( change.FilePath, out _ );

                            break;
                    }
                }
            }
        }

        public void Clear()
        {
            Logger.Instance?.Write( $"DesignTimeSyntaxTreeResultCache.Clear()." );
            this._syntaxTreeCache.Clear();
        }
    }
}