// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Exposes the <see cref="ISymbol"/> from <see cref="IDeclaration"/>.
    /// </summary>
    public static class CodeModelExtensions
    {
        public static ISymbol? GetSymbol( this IDeclaration declaration ) => ((ISdkDeclaration) declaration).Symbol;
        
        private static T? GetSymbol<T>( this IDeclaration declaration )
            where T : ISymbol
            => (T?) ((ISdkDeclaration) declaration).Symbol;

        public static ITypeSymbol GetSymbol( this IType type )
            => ((ISdkType) type).TypeSymbol ?? throw new InvalidOperationException( "Assertion failed: until type introductions are supported, all types are assumed to have a Roslyn symbol." );
        
        public static INamedTypeSymbol GetSymbol( this INamedType namedType )
            => namedType.GetSymbol<INamedTypeSymbol>() ?? throw new InvalidOperationException( "Assertion failed: uUntil type introductions are supported, all types are assumed to have a Roslyn symbol." );

        public static IMethodSymbol? GetSymbol( this IMethodBase method ) => method.GetSymbol<IMethodSymbol>();

        public static IPropertySymbol? GetSymbol( this IProperty property ) => property.GetSymbol<IPropertySymbol>();
        

    }
}