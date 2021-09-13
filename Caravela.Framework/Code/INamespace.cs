// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a namespace.
    /// </summary>
    public interface INamespace : INamedDeclaration
    {
        /// <summary>
        /// Gets the full name of the namespace, or an empty string if this is the global namespace.
        /// </summary>
        string FullName { get; }

        bool IsGlobalNamespace { get; }

        /// <exclude/>
        [Obsolete( "Not implemented." )]
        INamespace? ParentNamespace { get; }

        /// <exclude/>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamedType> Types { get; }

        /// <exclude/>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<INamespace> ChildrenNamespaces { get; }
    }
}