// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="GetTarget"/>,
    /// given an compilation, or using the <see cref="RefExtensions.GetTarget{T}"/> extension method
    /// for the compilation of the current context.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    public interface IRef<out T>
        where T : class, ICompilationElement
    {
        /// <summary>
        /// Returns a string that uniquely identifies the declaration represented by the current reference. This identifier can then be resolved using <see cref="IDeclarationFactory.GetDeclarationFromSerializableId"/>, even in
        /// a different process or with a different version of Metalama than the one that created the id.
        /// </summary>
        /// <returns>A string, or <c>null</c> if the current reference cannot be serialized to a public id.</returns>
        DeclarationSerializableId ToSerializableId();

        /// <summary>
        /// Gets the target of the reference for a given compilation. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
        /// </summary>
        T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default );
    }

    [Flags]
    public enum ReferenceResolutionOptions
    {
        Default,
        CanBeMissing,
        DoNotFollowRedirections,
    }
}