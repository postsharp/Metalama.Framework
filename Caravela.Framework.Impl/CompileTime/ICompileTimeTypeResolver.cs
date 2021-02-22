using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal interface ICompileTimeTypeResolver
    {
        Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock );
    }
}