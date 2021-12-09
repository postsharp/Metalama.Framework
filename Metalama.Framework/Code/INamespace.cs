// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a namespace inside the current compilation. Note that you cannot get the <see cref="INamespace"/>
    /// for an assembly outside of the current compilation.
    /// </summary>
    public interface INamespace : INamedDeclaration
    {
        /// <summary>
        /// Gets the full name of the namespace, or an empty string if this is the global namespace.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets a value indicating whether the current namespace represents the global (or root) namespace.
        /// </summary>
        bool IsGlobalNamespace { get; }

        /// <summary>
        /// Gets the parent namespace, or <c>null</c> if <see cref="IsGlobalNamespace"/> is <c>true</c>.
        /// </summary>
        INamespace? ParentNamespace { get; }

        /// <summary>
        /// Gets the list of types defined in the current namespace inside the current assembly, but not in descendant namespaces.
        /// </summary>
        INamedTypeList Types { get; }

        /// <summary>
        /// Gets the list of types defined in the current namespace and in all descendant namespaces in the current assembly.
        /// </summary>
        INamedTypeList AllTypes { get; }

        /// <summary>
        /// Gets the list of children namespaces of the current namespace in the current assembly.
        /// </summary>
        INamespaceList Namespaces { get; }
    }
}