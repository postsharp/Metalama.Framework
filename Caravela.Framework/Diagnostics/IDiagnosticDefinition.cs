// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A base interface for <see cref="DiagnosticDefinition"/> and <see cref="DiagnosticDefinition{T}"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface IDiagnosticDefinition
    {
        /// <summary>
        /// Gets the severity of the diagnostic.
        /// </summary>
        Severity Severity { get; }

        /// <summary>
        /// Gets an unique identifier for the diagnostic (e.g. <c>MY001</c>).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the formatting string of the diagnostic message.
        /// </summary>
        string MessageFormat { get; }

        /// <summary>
        /// Gets the category of the diagnostic (e.g. your product name).
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Gets a short title describing the diagnostic. This title is typically described in the solution explorer of the IDE
        /// and does not contain formatting string parameters.
        /// </summary>
        string Title { get; }
    }
}