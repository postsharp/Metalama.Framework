// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Templating
{
    internal static class SymbolAnnotationMapper
    {
        private static readonly ConditionalWeakTable<SyntaxAnnotation, ISymbol> _annotationToSymbolMap = new();
        private static readonly ConditionalWeakTable<ISymbol, List<SyntaxAnnotation>> _symbolToAnnotationsMap = new();

        public static SyntaxAnnotation GetOrCreateAnnotation( string kind, ISymbol symbol )
        {
            var list = _symbolToAnnotationsMap.GetOrCreateValue( symbol );

            lock ( list )
            {
                var annotation = list.SingleOrDefault( x => x.Kind == kind );

                if ( annotation == null )
                {
                    annotation = new SyntaxAnnotation( kind );
                    list.Add( annotation );
                    _annotationToSymbolMap.Add( annotation, symbol );
                }

                return annotation;
            }
        }

        public static ISymbol GetSymbolFromAnnotation( SyntaxAnnotation annotation )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_annotationToSymbolMap.TryGetValue( annotation, out var symbol ) )
            {
                throw new KeyNotFoundException();
            }

            return symbol;
        }
    }
}