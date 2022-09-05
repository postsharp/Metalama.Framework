// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Encapsulates an <see cref="IDiagnosticSink"/> and the default target of diagnostics, suppressions, and code fixes.
/// </summary>
/// <seealso href="@diagnostics"/>
public readonly struct ScopedDiagnosticSink : IDiagnosticSink
{
    private readonly IDiagnosticSink _sink;

    /// <summary>
    /// Gets the declaration on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDeclaration DefaultTargetDeclaration { get; }

    /// <summary>
    /// Gets the location on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDiagnosticLocation DefaultTargetLocation { get; }

    internal ScopedDiagnosticSink( IDiagnosticSink sink, IDiagnosticLocation defaultTargetLocation, IDeclaration defaultTargetDeclaration )
    {
        this._sink = sink;
        this.DefaultTargetLocation = defaultTargetLocation;
        this.DefaultTargetDeclaration = defaultTargetDeclaration;
    }

    /// <summary>
    /// Reports a diagnostic to the default location of the current <see cref="ScopedDiagnosticSink"/>..
    /// </summary>
    /// <param name="diagnostic"></param>
    public void Report( IDiagnostic diagnostic ) => this._sink.Report( diagnostic, this.DefaultTargetLocation );

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    /// <param name="suppression"></param>
    public void Suppress( SuppressionDefinition suppression ) => this._sink.Suppress( suppression, this.DefaultTargetDeclaration );

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    public void Suggest( CodeFix codeFix ) => this._sink.Suggest( codeFix, this.DefaultTargetDeclaration );

    /// <inheritdoc />
    public void Report( IDiagnostic diagnostic, IDiagnosticLocation? location ) => this._sink.Report( diagnostic, location );

    /// <inheritdoc />
    public void Suppress( SuppressionDefinition suppression, IDeclaration scope ) => this._sink.Suppress( suppression, scope );

    /// <inheritdoc />
    public void Suggest( CodeFix codeFix, IDiagnosticLocation location ) => this._sink.Suggest( codeFix, location );
}