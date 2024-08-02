// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="GetTarget"/>,
    /// given an compilation, or using the <see cref="RefExtensions.GetTarget{T}"/> extension method
    /// for the compilation of the current context.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    [CompileTime]
    [InternalImplement]
    public interface IRef<out T> : IEquatable<IRef<ICompilationElement>>
        where T : class, ICompilationElement
    {
        /// <summary>
        /// Returns a string that uniquely identifies the declaration represented by the current reference. This identifier can then be resolved using <see cref="IDeclarationFactory.GetDeclarationFromId"/>, even in
        /// a different process or with a different version of Metalama than the one that created the id.
        /// </summary>
        /// <returns>A string, or <c>null</c> if the current reference cannot be serialized to a public id.</returns>
        SerializableDeclarationId ToSerializableId();

        /// <summary>
        /// Gets the target of the reference for a given compilation, or throws an exception if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTargetOrNull{T}"/> extension method.
        /// </summary>
        T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default );

        /// <summary>
        /// Gets the target of the reference for a given compilation, or returns <c>null</c> if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
        /// </summary>
        T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default );

        IRef<TOut> As<TOut>()
            where TOut : class, ICompilationElement;

        bool Equals( IRef<ICompilationElement>? other, bool includeNullability );
    }
}