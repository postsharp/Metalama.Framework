// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<Compilation, DesignTimeAspectPipelineResult> _cache = new();

        public static bool TryGet( Compilation compilation, [NotNullWhen( true )] out DesignTimeAspectPipelineResult? result )
            => _cache.TryGetValue( compilation, out result );

        public static void Add( Compilation compilation, DesignTimeAspectPipelineResult pipelineResult ) => _cache.Add( compilation, pipelineResult );
    }
}