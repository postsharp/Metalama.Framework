// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// Defines the suppression of a kind of diagnostics. Suppressions must be
    /// defined as static fields or properties of an aspect classes. Suppressions are instantiated with <see cref="IDiagnosticSink.Suppress"/>.
    /// </summary>
    [CompileTimeOnly]
    public sealed class SuppressionDefinition
    {
        /// <summary>
        /// Gets the identifier of the suppression (not to be confused with <see cref="SuppressedDiagnosticId"/>).
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the ID of the diagnostic to be suppressed (e.g. <c>CS0169</c>).
        /// </summary>
        public string SuppressedDiagnosticId { get; }

        /// <summary>
        /// Gets an optional justification.
        /// </summary>
        public string? Justification { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressionDefinition"/> class.
        /// </summary>
        /// <param name="id">The identifier of the suppression itself (not to be confused with <paramref name="suppressedDiagnosticId"/>).</param>
        /// <param name="suppressedDiagnosticId">The ID of the diagnostic to be suppressed (e.g. <c>CS0169</c>).</param>
        /// <param name="justification">An optional justification.</param>
        public SuppressionDefinition( string id, string suppressedDiagnosticId, string? justification = null )
        {
            this.Id = id;
            this.SuppressedDiagnosticId = suppressedDiagnosticId;
            this.Justification = justification;
        }
    }
}