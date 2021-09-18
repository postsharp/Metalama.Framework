// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Allows to add members to an attribute created by <see cref="IDeclarationBuilder.AddAttribute"/>.
    /// </summary>
    public interface IAttributeBuilder : IAttribute
    {
        /// <summary>
        /// Adds a new named argument to the attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddNamedArgument( string name, object? value );
    }
}