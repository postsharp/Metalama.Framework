// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

public static class RefExtensions
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    public static SyntaxTree? GetPrimarySyntaxTree<T>( this T reference, CompilationContext compilationContext )
        where T : IRef<IDeclaration>
        => ((IRefImpl) reference).GetClosestSymbol( compilationContext ).GetPrimarySyntaxReference()?.SyntaxTree;

    // ReSharper disable once IdentifierTypo
    [return: NotNullIfNotNull( nameof(reference) )]
    internal static IRef<TTo>? As<TFrom, TTo>( this IRef<TFrom>? reference )
        where TFrom : class, ICompilationElement
        where TTo : class, ICompilationElement
        => reference switch
        {
            null => null,
            IRef<TTo> iref => iref,
            Ref<TFrom> @ref => @ref.As<TTo>(),
            _ => throw new InvalidOperationException( $"Cannot cast {reference.GetType()} to {typeof(IRef<TTo>)}." )
        };
}