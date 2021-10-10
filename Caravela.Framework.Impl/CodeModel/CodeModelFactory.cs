// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class CodeModelFactory
    {
        internal static IType CreateIType( ITypeSymbol typeSymbol, CompilationModel compilation )
            => typeSymbol switch
            {
                INamedTypeSymbol namedType => compilation.Factory.GetNamedType( namedType ),
                IArrayTypeSymbol arrayType => new ArrayType( arrayType, compilation ),
                IPointerTypeSymbol pointerType => new PointerType( pointerType, compilation ),
                ITypeParameterSymbol typeParameter => new TypeParameter( typeParameter, compilation ),
                IDynamicTypeSymbol dynamicType => new DynamicType( dynamicType, compilation ),
                _ => throw new NotImplementedException()
            };
    }
}