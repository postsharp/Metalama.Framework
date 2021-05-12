// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Caches the pipeline results for each syntax tree.
    /// </summary>
    internal sealed class DesignTimeSyntaxTreeResultCache
    {
        /// <summary>
        /// Maps the syntax tree name to the pipeline result for this syntax tree.
        /// </summary>
        private readonly ConcurrentDictionary<string, DesignTimeSyntaxTreeResult> _syntaxTreeCache = new();

        public int Count => this._syntaxTreeCache.Count;

        /// <summary>
        /// Updates cache with a <see cref="DesignTimeAspectPipelineResult"/> that includes results for several syntax trees.
        /// </summary>
        public void Update( Compilation compilation, DesignTimeAspectPipelineResult results )
        {
            var resultsByTree = SplitResultsByTree( compilation, results );

            foreach ( var result in resultsByTree )
            {
                if ( !Path.IsPathRooted( result.SyntaxTree.FilePath ) )
                {
                    throw new AssertionFailedException( "A rooted path was expected." );
                }

                this._syntaxTreeCache[result.SyntaxTree.FilePath] = result;
            }
        }

        /// <summary>
        /// Splits a <see cref="DesignTimeAspectPipelineResult"/>, which includes data for several syntax trees, into
        /// a list of <see cref="DesignTimeSyntaxTreeResult"/> which each have information related to a single syntax tree.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static IEnumerable<DesignTimeSyntaxTreeResult> SplitResultsByTree( Compilation compilation, DesignTimeAspectPipelineResult results )
        {
            var resultsByTree = results
                .InputSyntaxTrees
                .ToDictionary( r => r.FilePath, syntaxTree => new DesignTimeSyntaxTreeResultBuilder( syntaxTree ) );

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

            return resultsByTree.Select( b => b.Value.ToImmutable( compilation ) );
        }

        public bool TryGetValue( SyntaxTree syntaxTree, [NotNullWhen( true )] out DesignTimeSyntaxTreeResult? result )
        {
            return this._syntaxTreeCache.TryGetValue( syntaxTree.FilePath, out result );
        }

        public void UpdateCompilation( CompilationChanges compilationChanges )
        {
            foreach ( var change in compilationChanges.SyntaxTreeChanges )
            {
                switch ( change.SyntaxTreeChangeKind )
                {
                    case SyntaxTreeChangeKind.Added:
                        break;

                    case SyntaxTreeChangeKind.Deleted:
                    case SyntaxTreeChangeKind.Changed:
                        DesignTimeLogger.Instance?.Write( $"DesignTimeSyntaxTreeResultCache.InvalidateCache({change.FilePath}): removed from cache." );
                        this._syntaxTreeCache.TryRemove( change.FilePath, out _ );

                        break;
                }
            }
        }

        public void Clear()
        {
            DesignTimeLogger.Instance?.Write( $"DesignTimeSyntaxTreeResultCache.Clear()." );
            this._syntaxTreeCache.Clear();
        }
    }
}