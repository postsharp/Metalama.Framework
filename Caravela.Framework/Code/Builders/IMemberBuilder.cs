// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Builders
{
    /// <summary>
    /// Allows to complete the construction of a member (but not a named type) that has been created by an advice.
    /// </summary>
    public interface IMemberBuilder : IMemberOrNamedTypeBuilder, IMember
    {
        /// <summary>
        /// Gets or sets a value indicating whether the member is <c>virtual</c>.
        /// </summary>
        new bool IsVirtual { get; set; }
    }
}