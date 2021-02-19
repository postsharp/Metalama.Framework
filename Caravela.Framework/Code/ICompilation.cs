using Caravela.Framework.Project;
using System.Collections.Generic;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a set of types compiled together. Commonly known as a "project", but this is not exactly it.
    /// </summary>
    [CompileTime]
    public interface ICompilation : ICodeElement
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
    }
}