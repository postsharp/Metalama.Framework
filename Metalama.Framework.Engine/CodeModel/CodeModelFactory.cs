// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    [UsedImplicitly] // Reference not detected.
    public static class CodeModelFactory
    {
        [UsedImplicitly]
        public static ICompilation CreateCompilation(
            Compilation compilation,
            ProjectServiceProvider serviceProvider,
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