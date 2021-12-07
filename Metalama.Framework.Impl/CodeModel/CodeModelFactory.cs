// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
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
                _ => throw new NotImplementedException()
            };
    }
}