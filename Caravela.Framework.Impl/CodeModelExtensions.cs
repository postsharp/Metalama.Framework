using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    static class CodeModelExtensions
    {
        // TODO: should this be in the SDK?
        public static INamedTypeSymbol GetSymbol( this INamedType namedType ) => ((NamedType) namedType).TypeSymbol;
        public static ITypeSymbol GetSymbol( this IType type ) => ((ITypeInternal) type).TypeSymbol;
    }
}
