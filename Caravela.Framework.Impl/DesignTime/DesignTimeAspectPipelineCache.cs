// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<object, DesignTimeAspectPipelineResult> _cache = new();
        private static readonly ConditionalWeakTable<object, object> _sync = new();
        

        public static DesignTimeAspectPipelineResult GetPipelineResult(
            SemanticModel semanticModel,
            AnalyzerBuildOptionsSource buildOptionsSource,
            CancellationToken cancellationToken )
            => GetPipelineResult( semanticModel, new BuildOptions( buildOptionsSource ), cancellationToken );

        public static DesignTimeAspectPipelineResult GetPipelineResult(
            SemanticModel semanticModel,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            // ReSharper disable once InconsistentlySynchronizedField

            if ( !_cache.TryGetValue( semanticModel, out var result ) )
            {
                var lockable = _sync.GetOrCreateValue( semanticModel );

                lock ( lockable )
                {
                    if ( !_cache.TryGetValue( semanticModel, out result ) )
                    {
                        using DesignTimeAspectPipeline pipeline = new( buildOptions );

                        result = pipeline.AnalyzeSemanticModel( semanticModel );

                        _cache.Add( semanticModel, result );
                    }
                }
            }

            return result;
        }
        
        public static DesignTimeAspectPipelineResult GetPipelineResult(
            Compilation compilation,
            AnalyzerBuildOptionsSource buildOptionsSource,
            CancellationToken cancellationToken )
            => GetPipelineResult( compilation, new BuildOptions( buildOptionsSource ), cancellationToken );

        public static DesignTimeAspectPipelineResult GetPipelineResult(
            Compilation compilation,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            
            // ReSharper disable once InconsistentlySynchronizedField

            if ( !_cache.TryGetValue( compilation, out var result ) )
            {
                var lockable = _sync.GetOrCreateValue( compilation );

                lock ( lockable )
                {
                    if ( !_cache.TryGetValue( compilation, out result ) )
                    {
                        using DesignTimeAspectPipeline pipeline = new( buildOptions );

                        result = pipeline.AnalyzeCompilation( compilation );

                        // Add the result to the cache for all semantic models.
                        foreach ( var syntaxTree in compilation.SyntaxTrees )
                        {
                            var semanticModel = compilation.GetSemanticModel( syntaxTree );
                            _cache.Add( semanticModel, result );
                        }

                        _cache.Add( compilation, result );
                    }
                }
            }

            return result;
        }
        
    }
}