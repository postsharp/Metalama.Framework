// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Defines the suppression of a kind of diagnostics. Suppressions must be
/// defined as static fields or properties of an aspect classes. Suppressions are instantiated with <see cref="IDiagnosticSink.Suppress"/>.
/// </summary>
/// <seealso href="@diagnostics"/>
[CompileTime]
[PublicAPI]
public sealed class SuppressionDefinition : ISuppression
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

    SuppressionDefinition ISuppression.Definition => this;

    Func<SuppressionDiagnostic, bool>? ISuppression.Filter => null;

    /// <summary>
    /// Returns a new instance of the current suppression with a filter that will be applied to the diagnostics.
    /// </summary>
    public ISuppression WithFilter( Func<SuppressionDiagnostic, bool> filter ) => new SuppressionImpl( this, filter );

    public override string ToString() => $"suppress {this.SuppressedDiagnosticId}";
}