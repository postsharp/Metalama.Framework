﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal class CodeElementLinkEqualityComparer<T> : IEqualityComparer<T>
        where T : ICodeElementLink
    
    {
        public static readonly CodeElementLinkEqualityComparer<T> Instance = new CodeElementLinkEqualityComparer<T>();

        private static ISymbol? GetSymbol( T link )
            => link.LinkedObject switch
            {
                ISymbol symbol => symbol,
                CodeElement codeElement => codeElement.Symbol,
                _ => null
            };

        public bool Equals( T x, T y )
        {
            var xSymbol = GetSymbol( x );
            if ( xSymbol != null )
            {
                return SymbolEqualityComparer.Default.Equals( xSymbol, GetSymbol( y ) );
            }
            else
            {
                return ReferenceEquals( x.LinkedObject, y.LinkedObject );
            }
        }


        public int GetHashCode( T? obj )
        {
            if ( obj == null )
            {
                return 0;
            }
            else
            {
                var xSymbol = GetSymbol( obj );
                if ( xSymbol != null )
                {
                    return SymbolEqualityComparer.Default.GetHashCode( xSymbol );
                }
                else
                {
                    return RuntimeHelpers.GetHashCode( obj.LinkedObject );
                }
            }

        }
    }
}
