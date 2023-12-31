// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Represents an instance of a diagnostic, including its parameters and its optional code fixes.
/// </summary>
/// <seealso href="@diagnostics"/>
[CompileTime]
[InternalImplement]
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
    /// Adds a set of code fixes to the current instance, and returns the current augmented instance. To create a one-step
    /// code fix, use the <see cref="CodeFixFactory"/> class.
    /// </summary>
    IDiagnostic WithCodeFixes( params CodeFix[] codeFixes );
}