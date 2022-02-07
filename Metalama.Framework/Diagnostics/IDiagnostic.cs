// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Represents an instance of a diagnostic, including its parameters and its optional code fixes.
/// </summary>
public interface IDiagnostic
{
    /// <summary>
    /// Gets the <see cref="IDiagnosticDefinition"/> from which the current diagnostic has been created.
    /// </summary>
    IDiagnosticDefinition Definition { get; }

    /// <summary>
    /// Gets the set of code fixes for the current diagnostic.
    /// </summary>
    ImmutableArray<CodeFix> CodeFixes { get; }

    /// <summary>
    /// Gets the arguments of the current diagnostic.
    /// </summary>
    object? Arguments { get; }

    /// <summary>
    /// Adds a set of code fixes to the current instance, and returns the current augmented instance.
    /// </summary>
    IDiagnostic WithCodeFixes( params CodeFix[] codeFixes );
}