// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Allows to complete the construction of a declaration that has been created by an advice.
    /// </summary>
    public interface IDeclarationBuilder : IDeclaration
    {
        bool IsFrozen { get; }

        void Freeze();

        /// <summary>
        /// Adds a custom attribute to the current declaration.
        /// </summary>
        void AddAttribute( AttributeConstruction attribute );

        // TODO: There is no way to provide the value of an enum when the enum type is run-time-only.

        /// <summary>
        /// Removes all custom attributes of a given type from the current declaration.
        /// </summary>
        /// <param name="type">TYpe of custom attributes to be removed.</param>
        void RemoveAttributes( INamedType type );
    }
}