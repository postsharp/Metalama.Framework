using System.Collections.Generic;
using Caravela.Framework.Project;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a set of types compiled together. Commonly known as a "project", but this is not exactly it.
    /// </summary>
    [CompileTime]
    public interface ICompilation : IAssembly
    {
        /// <summary>
        /// Gets the list of types declared in the current compilation, in all namespaces, but not the nested types.
        /// </summary>
        INamedTypeList DeclaredTypes { get; }

        /// <summary>
        /// Gets a service that allows to create type instances and compare them.
        /// </summary>
        ITypeFactory TypeFactory { get; }

        /// <summary>
        /// Gets the list of managed resources in the current compilation.
        /// </summary>
        IReadOnlyList<IManagedResource> ManagedResources { get; }

        /// <summary>
        /// Gets a service allowing to compare types and code elements considers equal two instances that represent
        /// the same type or code element even if they belong to different compilation versions.
        /// </summary>
        ICodeElementComparer InvariantComparer { get; }
    }
}