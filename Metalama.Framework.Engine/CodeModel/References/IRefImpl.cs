// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

// ReSharper disable UnusedMemberInSuper.Global

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// A weakly typed base for <see cref="ISdkRef{T}"/>.
    /// </summary>
    internal interface IRefImpl : IRef
    {
        string Name { get; }

        IRefStrategy Strategy { get; }

        CompilationContext CompilationContext { get; }

        ISymbol GetClosestSymbol();

        (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData();

        IRefImpl Unwrap();
    }

    internal interface IRefImpl<out T> : ISdkRef<T>, IRefImpl
        where T : class, ICompilationElement
    {
        new IRefImpl<TOut> As<TOut>()
            where TOut : class, ICompilationElement;
    }
}