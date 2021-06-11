// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Builders
{
    /// <summary>
    /// Allows to complete the construction of a member that has been created by an advice.
    /// </summary>
    public interface IMemberOrNamedTypeBuilder : IMemberOrNamedType, IDeclarationBuilder
    {
        /// <summary>
        /// Gets or sets the accessibility of the member.
        /// </summary>
        new Accessibility Accessibility { get; set; }

        /// <summary>
        /// Gets or sets the member name.
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>static</c>.
        /// </summary>
        new bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>sealed</c>.
        /// </summary>
        new bool IsSealed { get; set; }
    }

    public interface IMemberBuilder : IMemberOrNamedTypeBuilder, IMember
    {
        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        new bool IsVirtual { get; set; }
    }
}