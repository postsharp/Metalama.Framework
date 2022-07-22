// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    public static class CodeModelFactory
    {
        public static ICompilation CreateCompilation(
            Compilation compilation,
            IServiceProvider serviceProvider,
            ImmutableArray<ManagedResource> resources = default )
        {
            var partialCompilation = PartialCompilation.CreateComplete( compilation, resources );
            var projectModel = new ProjectModel( compilation, serviceProvider );

            return CompilationModel.CreateInitialInstance( projectModel, partialCompilation );
        }

        internal static IType CreateIType( ITypeSymbol typeSymbol, CompilationModel compilation )
            => typeSymbol switch
            {
                INamedTypeSymbol namedType => compilation.Factory.GetNamedType( namedType ),
                IArrayTypeSymbol arrayType => new ArrayType( arrayType, compilation ),
                IPointerTypeSymbol pointerType => new PointerType( pointerType, compilation ),
                ITypeParameterSymbol typeParameter => new TypeParameter( typeParameter, compilation ),
                IDynamicTypeSymbol dynamicType => new DynamicType( dynamicType, compilation ),
                IFunctionPointerTypeSymbol functionPointerType => new FunctionPointerType( functionPointerType, compilation ),
                _ => throw new NotImplementedException( $"Types of kind {typeSymbol.Kind} are not implemented." )
            };
    }
}