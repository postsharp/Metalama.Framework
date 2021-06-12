// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

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
        /// Gets the ID of the diagnostic to be suppressed (e.g. <c>CS0169</c>).
        /// </summary>
        public string SuppressedDiagnosticId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppressionDefinition"/> class.
        /// </summary>
        /// <param name="suppressedDiagnosticId">The ID of the diagnostic to be suppressed (e.g. <c>CS0169</c>).</param>
        public SuppressionDefinition( string suppressedDiagnosticId )
        {
            this.SuppressedDiagnosticId = suppressedDiagnosticId;
        }
    }
}