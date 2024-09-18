// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    /// <summary>
    /// Removes any <see cref="CastRef{T}"/> wrapper.
    /// </summary>
    internal static IRefImpl Unwrap( this IRef reference ) => ((IRefImpl) reference).Unwrap();

    internal static IRefImpl<T> AsRefImpl<T>( this IRef<T> reference )
        where T : class, ICompilationElement
        => (IRefImpl<T>) reference;

    internal static IRefImpl<T> ToRefImpl<T>( this T declaration )
        where T : class, IDeclaration
        => (IRefImpl<T>) declaration.ToRef();

    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree<T>( this T reference, CompilationContext compilationContext )
        where T : IRef<IDeclaration>
        => ((IRefImpl) reference).GetClosestSymbol( compilationContext ).GetPrimarySyntaxReference()?.SyntaxTree;
}