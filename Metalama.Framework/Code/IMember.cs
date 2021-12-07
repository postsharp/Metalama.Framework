// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IFieldOrProperty"/>, <see cref="IEvent"/>, but not <see cref="INamedType"/>.
    /// </summary>
    public interface IMember : IMemberOrNamedType
    {
        /// <summary>
        /// Gets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>async</c>.
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>override</c>.
        /// </summary>
        bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the member is an explicit implementation of an interface member.
        /// </summary>
        bool IsExplicitInterfaceImplementation { get; }

        /// <summary>
        /// Gets the type containing the current member, or <c>null</c> if the current member is not contained
        /// within a type (which should not happen in C#).
        /// </summary>
        new INamedType DeclaringType { get; }
    }
}