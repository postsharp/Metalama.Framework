// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static partial class DesignTimeAspectPipelineCache
    {
        private static class ResultCache
        {
            private static readonly ConditionalWeakTable<object, DesignTimeAspectPipelineResult> _cache = new();

            public static void Update( SemanticModel semanticModel, DesignTimeAspectPipelineResult value ) => UpdateImpl( semanticModel, value );
            public static void Update( Compilation compilation, DesignTimeAspectPipelineResult value ) => UpdateImpl( compilation, value );

            public static bool TryGetValue( SemanticModel semanticModel, [NotNullWhen( true )]  out DesignTimeAspectPipelineResult? result ) 
                => _cache.TryGetValue( semanticModel, out result ) || _cache.TryGetValue( semanticModel.Compilation, out result );
            public static bool TryGetValue( Compilation compilation, [NotNullWhen(true)] out DesignTimeAspectPipelineResult? result ) => _cache.TryGetValue( compilation, out result );
            
            private static void UpdateImpl( object key, DesignTimeAspectPipelineResult value )
            {
                if ( _cache.TryGetValue( key, out var currentValue ) && currentValue == value)
                {
                    return;
                }

                while ( true )
                {
                    _cache.Remove( key );

                    try
                    {
                        _cache.Add( key, value );
                        return;
                    }
                    catch ( ArgumentException )
                    {
                    
                    }
                }
            }

            
        }
    }
}