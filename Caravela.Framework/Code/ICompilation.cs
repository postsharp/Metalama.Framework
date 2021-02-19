using System.Collections.Generic;
using Caravela.Framework.Collections;
using Caravela.Framework.Project;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a set of types compiled together. Commonly known as a "project", but this is not exactly it.
    /// </summary>
    [CompileTime]
    public interface ICompilation : ICodeElement
    {
        int Revision { get; }
        
        INamedTypeList DeclaredTypes { get; }

        ITypeFactory TypeFactory { get; }

        IReadOnlyList<IManagedResource> ManagedResources { get; }

        
    }
}
