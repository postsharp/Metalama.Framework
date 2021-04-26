// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<Compilation, DesignTimeAspectPipelineResult> _cache = new();
        private static readonly ConditionalWeakTable<Compilation, object> _sync = new();

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
                        using DesignTimeAspectPipeline pipeline = new( new DesignTimeAspectPipelineContext(
                                                                           (CSharpCompilation) compilation,
                                                                           buildOptions,
                                                                           cancellationToken ) );

                        _ = pipeline.TryExecute( out result );

                        _cache.Add( compilation, result );
                    }
                }
            }

            return result;
        }
    }
}