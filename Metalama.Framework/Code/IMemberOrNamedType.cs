// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Base interface for <see cref="IMethod"/>, <see cref="IFieldOrProperty"/>, <see cref="IEvent"/>, and <see cref="INamedType"/>.
    /// </summary>
    public interface IMemberOrNamedType : INamedDeclaration
    {
        /// <summary>
        /// Gets the member accessibility (or visibility), i.e. <see cref="Code.Accessibility.Private"/>, <see cref="Code.Accessibility.Protected"/>
        /// and so on.
        /// </summary>
        Accessibility Accessibility { get; }

        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>static</c>.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the member is <c>sealed</c>.
        /// </summary>
        /// <seealso cref="MemberExtensions.IsOverridable"/>
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
        [CompileTimeReturningRunTime]
        MemberInfo ToMemberInfo();
    }
}