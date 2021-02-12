using Caravela.Framework.Collections;
using System;
using System.Collections.Generic;
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
        IReadOnlyList<INamedType> DeclaredTypes { get; }

        IReadOnlyList<INamedType> DeclaredAndReferencedTypes { get; }

        IReadOnlyMultiValueDictionary<string?, INamedType> DeclaredTypesByNamespace { get; }
        
        ITypeFactory TypeFactory { get; }
        
        IReadOnlyList<IManagedResource> ManagedResources { get; }
        
        IReadOnlyMultiValueDictionary<INamedType, IAttribute> AllAttributesByType { get; }

      
    }
}
