using System;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="ICompileTimeTypeResolver"/> that cannot be used for user-code attributes.
    /// </summary>
    internal class SystemTypeResolver : ICompileTimeTypeResolver
    {
        public Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock )
        {
            var typeName = ReflectionNameHelper.GetReflectionName( typeSymbol );
            if ( typeSymbol.ContainingAssembly != null )
            {
                typeName += ", " + typeSymbol.ContainingAssembly.Name;
            }

            var type = Type.GetType( typeName );
            if ( type == null )
            {
                if ( fallbackToMock )
                {
                    return new CompileTimeType( typeSymbol );
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return type;
            }
        }
    }
}