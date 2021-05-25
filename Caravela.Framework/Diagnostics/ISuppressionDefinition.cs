// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Diagnostics
{
    public interface ISuppressionDefinition
    {
        /// <summary>
        /// Gets the ID of the diagnostic to be suppressed (e.g. <c>CS0169</c>).
        /// </summary>
        string SuppressedDiagnosticId { get; }
    }
}