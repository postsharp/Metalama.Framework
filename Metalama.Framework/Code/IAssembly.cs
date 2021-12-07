// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents an assembly (typically a reference assembly).
    /// </summary>
    public interface IAssembly : IDeclaration
    {
        /// <summary>
        /// Gets a value indicating whether the assembly represents a reference (<c>true</c>), or a project reference (<c>false</c>).
        /// </summary>
        bool IsExternal { get; }

        /// <summary>
        /// Gets the assembly identity.
        /// </summary>
        IAssemblyIdentity Identity { get; }
    }
}