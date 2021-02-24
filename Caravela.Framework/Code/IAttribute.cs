// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent a custom attributes.
    /// </summary>
    /// <remarks>
    /// Values of <see cref="ConstructorArguments"/> and <see cref="NamedArguments"/> are represented as:
    /// <list type="bullet">
    /// <item>Primitive types as themselves (e.g. int as int, string as string).</item>
    /// <item>Enums as their underlying type.</item>
    /// <item><see cref="System.Type"/> as <see cref="IType"/>.</item>
    /// <item>Arrays as <c>IReadOnlyList&lt;object&gt;</c>.</item>
    /// </list>
    /// </remarks>
    public interface IAttribute : ICodeElement
    {
        // TODO: add TargetElement?

        /// <summary>
        /// Gets the custom attribute type.
        /// </summary>
        INamedType Type { get; }

        /// <summary>
        /// Gets the constructor to be used to instantiate the custom attribute.
        /// </summary>
        IMethod Constructor { get; }

        /// <summary>
        /// Gets the parameters passed to the <see cref="Constructor"/>.
        /// </summary>
        IReadOnlyList<TypedConstant> ConstructorArguments { get; }

        /// <summary>
        /// Gets the named arguments (either fields or properties) of the attribute.
        /// </summary>
        INamedArgumentList NamedArguments { get; }
    }
}