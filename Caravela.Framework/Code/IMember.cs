// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IProperty"/> and <see cref="IEvent"/>, but not <see cref="INamedType"/>.
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

        new INamedType DeclaringType { get; }
    }
}