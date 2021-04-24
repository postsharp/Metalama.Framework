// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents an aspect type.
    /// </summary>
    public interface IAspectType
    {
        string Name { get; }

        INamedTypeSymbol TypeSymbol { get; }

        bool IsAbstract { get; }
    }
}