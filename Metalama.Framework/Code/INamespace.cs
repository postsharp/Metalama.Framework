// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a namespace inside the current compilation or an external assembly, according to the <see cref="IDeclaration.DeclaringAssembly"/> property.
    /// </summary>
    /// <remarks>
    /// At design time, namespaces of the current compilation can be partial, or incomplete. See <see cref="IsPartial"/> for details.
    /// </remarks>
    public interface INamespace : INamedTypeOrNamespace
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
        /// Gets the list of types defined in the current namespace inside the <see cref="IDeclaration.DeclaringAssembly"/>, but not in descendant namespaces.
        /// In case of partial compilations (see <see cref="IsPartial"/>), this collection only contain the types in the current
        /// partial compilation.
        /// </summary>
        INamedTypeCollection Types { get; }

        /// <summary>
        /// Gets the list of children namespaces of the current namespace the <see cref="IDeclaration.DeclaringAssembly"/>.
        /// In case of partial compilations (see <see cref="IsPartial"/>), this collection only contain the namespaces in the current
        /// partial compilation.
        /// </summary>
        INamespaceCollection Namespaces { get; }

        /// <summary>
        /// Gets a descendant of the current namespace.
        /// </summary>
        /// <param name="ns">Dot-separated name of the namespace relatively to the current namespace.</param>
        INamespace? GetDescendant( string ns );

        /// <summary>
        /// Gets a value indicating whether the current namespace is partial, i.e. incomplete. Metalama uses partial compilations
        /// at design time, when only the closure of modified types are being incrementally recompiled. In this scenario, namespaces
        /// of the current compilation are partial. Namespaces of external assemblies are never partial.
        /// </summary>
        bool IsPartial { get; }
    }
}