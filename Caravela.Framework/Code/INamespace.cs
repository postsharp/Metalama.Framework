using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public interface INamespace : IDeclaration
    {
        string Name { get; }
        
        string FullName { get; }
        
        INamespace? ParentNamespace { get; }
        
        IReadOnlyList<INamedType> Types { get; }

        IReadOnlyList<INamespace> ChildrenNamespaces { get; }

    }
}