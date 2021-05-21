// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to complete the construction of a declaration that has been created by an advice.
    /// </summary>
    public interface IDeclarationBuilder : IDeclaration
    {
        bool IsFrozen { get; }

        /// <summary>
        /// Adds a custom attribute to the current declaration.
        /// </summary>
        /// <param name="type">Type of the custom attribute.</param>
        /// <param name="constructorArguments">Arguments of the constructors.</param>
        /// <returns></returns>
        IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments );

        /// <summary>
        /// Removes all custom attributes of a given type from the current declaration.
        /// </summary>
        /// <param name="type">TYpe of custom attributes to be removed.</param>
        void RemoveAttributes( INamedType type );
    }
}