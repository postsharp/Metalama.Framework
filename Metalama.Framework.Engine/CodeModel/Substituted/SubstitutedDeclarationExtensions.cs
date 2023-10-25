// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal static class SubstitutedDeclarationExtensions
{
    [return: NotNullIfNotNull( nameof(typeSymbol) )]
    public static T? MapSymbol<T>( this ISubstitutedDeclaration declaration, T? typeSymbol )
        where T : class, ITypeSymbol
        => declaration.GenericMap.Map( typeSymbol );

    public static T MapIType<T>( this ISubstitutedDeclaration declaration, T type )
        where T : IType
        => (T) declaration.GetCompilationModel().Factory.GetIType( declaration.MapSymbol( type.GetSymbol() ) );
}