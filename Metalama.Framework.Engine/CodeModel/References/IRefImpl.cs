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

        /// <summary>
        /// Gets a value indicating whether the reference is portable to a different edition of the project. Most references are bound to a
        /// specific state of the project. They are faster to resolve but prevent that specific project state to be garbage-collected. Portable
        /// references are slower to resolve but not cause a memory leak if they stay in memory for a long time.
        /// </summary>
        bool IsPortable { get; }

        IRef ToPortable();

        ISymbol GetClosestContainingSymbol( CompilationContext compilationContext );
    }

    internal interface ICompilationBoundRefImpl : IRefImpl
    {
        CompilationContext CompilationContext { get; }

        (ImmutableArray<AttributeData> Attributes, ISymbol Symbol) GetAttributeData();
    }

    internal interface IRefImpl<out T> : ISdkRef<T>, IRefImpl
        where T : class, ICompilationElement
    {
        new IRefImpl<TOut> As<TOut>()
            where TOut : class, ICompilationElement;

        new IRef<T> ToPortable();
    }
}