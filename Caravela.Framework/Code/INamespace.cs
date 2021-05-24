// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public interface INamespace : IDeclaration
    {
        string Name { get; }

        string FullName { get; }

        [Obsolete( "Not implemented." )]
        INamespace? ParentNamespace { get; }

        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamedType> Types { get; }

        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamespace> ChildrenNamespaces { get; }
    }
}