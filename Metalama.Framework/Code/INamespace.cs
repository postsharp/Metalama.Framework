// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        /// In case of partial compilations (see <see cref="ICompilation.IsPartial"/>), this collection only contain the types in the current
        /// partial compilation.
        /// </summary>
        INamedTypeCollection Types { get; }

        /// <summary>
        /// Gets the list of types defined in the current namespace and in all descendant namespaces in the current assembly.
        /// In case of partial compilations (see <see cref="ICompilation.IsPartial"/>), this collection only contain the types in the current
        /// partial compilation.
        /// </summary>
        INamedTypeCollection AllTypes { get; }

        /// <summary>
        /// Gets the list of children namespaces of the current namespace in the current assembly.
        /// In case of partial compilations (see <see cref="ICompilation.IsPartial"/>), this collection only contain the namespaces in the current
        /// partial compilation.
        /// </summary>
        INamespaceCollection Namespaces { get; }

        /// <summary>
        /// Gets a value indicating whether the current namespace is purely external, i.e. if the current compilation
        /// does not contain any type or child namespace in this namespace, i.e. the <see cref="Types"/>, <see cref="AllTypes"/> and <see cref="Namespaces"/>
        /// collections are empty.
        /// </summary>
        bool IsExternal { get; }
    }
}