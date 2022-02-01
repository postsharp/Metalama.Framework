// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Introspection
{
    /// <summary>
    /// Represents a diagnostic (error, warning, information, hidden message).
    /// </summary>
    public interface IDiagnostic
    {
        ICompilation Compilation { get; }

        string Id { get; }

        string Message { get; }

        string? FilePath { get; }

        int? Line { get; }

        IDeclaration? Declaration { get; }

        Severity Severity { get; }
    }
}