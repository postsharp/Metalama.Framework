// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    public interface INamedDeclaration : IDeclaration
    {
        /// <summary>
        /// Gets the declaration name. If the member is an <see cref="INamedType"/> or <see cref="INamespace"/>, the <see cref="Name"/>
        /// property gets the short name of the type or namespace, without the parent namespace. See also <see cref="INamedType.Namespace"/>
        /// and <see cref="INamedType.FullName"/>.
        /// </summary>
        string Name { get; }
    }
}