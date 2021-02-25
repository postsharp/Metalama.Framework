using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal interface ICompileTimeTypeResolver
    {
        Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock );
    }
}