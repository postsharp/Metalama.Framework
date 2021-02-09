using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{

    internal static class CodeModelFactory
    {
        internal static ITypeInternal CreateIType( ITypeSymbol typeSymbol, SourceCompilationModel compilation ) =>
            typeSymbol switch
            {
                INamedTypeSymbol namedType => new SourceNamedType( compilation, namedType ),
                IArrayTypeSymbol arrayType => new SourceArrayType( compilation, arrayType ),
                IPointerTypeSymbol pointerType => new SourcePointerType( compilation, pointerType ),
                ITypeParameterSymbol typeParameter => new SourceGenericParameter( compilation, typeParameter),
                IDynamicTypeSymbol dynamicType => new SourceDynamicType( compilation, dynamicType ),
                _ => throw new NotImplementedException()
            };
    }
}
