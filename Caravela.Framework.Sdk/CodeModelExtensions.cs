// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Exposes the <see cref="ISymbol"/> from <see cref="IDeclaration"/>.
    /// </summary>
    public static class CodeModelExtensions
    {
        public static ISymbol? GetSymbol( this IDeclaration declaration ) => ((ISdkDeclaration) declaration).Symbol;

        public static ITypeSymbol? GetSymbol( this IType type ) => ((ISdkType) type).TypeSymbol;
    }
}