using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    internal static class CodeModelExtensions
    {
        // TODO: should this be in the SDK?
        public static INamedTypeSymbol GetSymbol( this INamedType namedType )
        {
            if ( namedType is SourceNamedType sourceNamedType )
            {
                return sourceNamedType.TypeSymbol;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(namedType), "This is not a source symbol.");
            }
        }

        public static ITypeSymbol GetSymbol( this IType type )
        {
            if ( type is ISourceType sourceNamedType )
            {
                return sourceNamedType.TypeSymbol;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type), "This is not a source symbol.");
            }
        }

        public static IMethodSymbol GetSymbol( this IMethod method )
        {
            if ( method is SourceMethod sourceMethod )
            {
                return (IMethodSymbol) sourceMethod.Symbol;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(method), "This is not a source symbol.");
            }
        }
    }
}
