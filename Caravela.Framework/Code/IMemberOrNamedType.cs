// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IProperty"/> and <see cref="IEvent"/>, but not <see cref="INamedType"/>.
    /// </summary>
    public interface IMemberOrNamedType : IDeclaration
    {
        /// <summary>
        /// Gets the member accessibility (or visibility), i.e. <see cref="Code.Accessibility.Private"/>, <see cref="Code.Accessibility.Protected"/>
        /// and so on.
        /// </summary>
        Accessibility Accessibility { get; }

        /// <summary>
        /// Gets the member name. If the member is a <see cref="INamedType"/>, the <see cref="Name"/>
        /// property gets the short name of the type, without the namespace. See also <see cref="INamedType.Namespace"/>
        /// and <see cref="INamedType.FullName"/>.
        /// </summary>
        string Name { get; }

        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>static</c>.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>sealed</c>.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>new</c>.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Gets the type containing the current member, or <c>null</c> if the current member is not contained
        /// within a type (which should not happen in C#).
        /// </summary>
        INamedType? DeclaringType { get; }

        /// <summary>
        /// Gets a <see cref="MemberInfo"/> that represents the current member at run time.
        /// </summary>
        /// <returns>A <see cref="MethodInfo"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        MemberInfo ToMemberInfo();
    }
}