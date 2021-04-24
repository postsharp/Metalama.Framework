// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    internal static class SymbolExtensions
    {
        public static ITypeSymbol? GetTypeByReflectionType( this Compilation compilation, Type type )
            => ReflectionMapper.GetInstance( compilation ).GetTypeSymbol( type );

        public static bool Is( this ITypeSymbol left, Type right )
        {
            if ( right.IsGenericType )
            {
                throw new ArgumentOutOfRangeException( nameof(right), "This method does not work with generic types." );
            }
            
            var rightName = right.FullName;

            if ( left.GetReflectionName() == rightName )
            {
                return true;
            }
            else if ( left.BaseType != null && left.BaseType.Is( right ) )
            {
                return true;
            }
            else
            {
                foreach ( var i in left.Interfaces )
                {
                    if ( i.Is( right ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}