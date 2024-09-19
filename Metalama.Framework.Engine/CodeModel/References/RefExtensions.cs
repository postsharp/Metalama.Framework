﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    /// <summary>
    /// Removes any <see cref="CastRef{T}"/> wrapper.
    /// </summary>
    internal static IRefImpl Unwrap( this IRef reference ) => ((IRefImpl) reference).Unwrap();

    internal static IRefStrategy GetStrategy( this IRef reference ) => reference.Unwrap().Strategy;

    internal static IRefImpl<T> AsRefImpl<T>( this IRef<T> reference )
        where T : class, ICompilationElement
        => (IRefImpl<T>) reference;

    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree<T>( this T reference )
        where T : IRef<IDeclaration>
        => ((IRefImpl) reference).GetClosestSymbol().GetPrimarySyntaxReference()?.SyntaxTree;
}