// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IFieldOrProperty"/>, <see cref="IEvent"/>, but not <see cref="INamedType"/>.
    /// </summary>
    public interface IMember : IMemberOrNamedType
    {
        /// <summary>
        /// Gets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        /// <seealso cref="MemberExtensions.IsOverridable"/>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>async</c>.
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>override</c>.
        /// </summary>
        /// <seealso cref="MemberExtensions.IsOverridable"/>
        bool IsOverride { get; }

        /// <summary>
        /// Gets a value indicating whether the member is an explicit implementation of an interface member.
        /// </summary>
        bool IsExplicitInterfaceImplementation { get; }

        /// <summary>
        /// Gets a value indicating whether the member has an implementation or is only a definition without a body.
        /// </summary>
        bool HasImplementation { get; }

        /// <summary>
        /// Gets the type containing the current member.
        /// </summary>
        new INamedType DeclaringType { get; }
    }
}