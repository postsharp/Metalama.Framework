// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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