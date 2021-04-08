// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A weakly-typed interface for <see cref="StrongDiagnosticDescriptor{T}"/>.
    /// </summary>
    internal interface IStrongDiagnosticDescriptor
    {
        DiagnosticSeverity Severity { get; }

        string Id { get; }

        string MessageFormat { get; }

        string Category { get; }
    }
}