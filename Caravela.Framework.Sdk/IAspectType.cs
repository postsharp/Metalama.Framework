// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents an aspect type.
    /// </summary>
    public interface IAspectType
    {
        string Name { get; }

        INamedType Type { get; }

        bool IsAbstract { get; }
    }
}
