// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Encapsulates an <see cref="IDiagnosticSink"/> and the default target of diagnostics, suppressions, and code fixes.
/// </summary>
/// <seealso href="@diagnostics"/>
[PublicAPI]
[CompileTime]
public readonly struct ScopedDiagnosticSink
{
    private readonly IDiagnosticSink _sink;
    private readonly IDiagnosticSource _source;

    /// <summary>
    /// Gets the declaration on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDeclaration DefaultTargetDeclaration { get; }

    /// <summary>
    /// Gets the location on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDiagnosticLocation DefaultTargetLocation { get; }

    internal ScopedDiagnosticSink(
        IDiagnosticSink sink,
        IDiagnosticSource source,
        IDiagnosticLocation defaultTargetLocation,
        IDeclaration defaultTargetDeclaration )
    {
        this._sink = sink;
        this._source = source;
        this.DefaultTargetLocation = defaultTargetLocation;
        this.DefaultTargetDeclaration = defaultTargetDeclaration;
    }

    /// <summary>
    /// Reports a diagnostic to the default location of the current <see cref="ScopedDiagnosticSink"/>..
    /// </summary>
    /// <param name="diagnostic"></param>
    public void Report( IDiagnostic diagnostic ) => this._sink.Report( diagnostic, this.DefaultTargetLocation, this._source );

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    /// <param name="suppression"></param>
    public void Suppress( SuppressionDefinition suppression ) => this._sink.Suppress( suppression, this.DefaultTargetDeclaration, this._source );

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    public void Suggest( CodeFix codeFix ) => this._sink.Suggest( codeFix, this.DefaultTargetDeclaration, this._source );

    /// <inheritdoc />
    public void Report( IDiagnostic diagnostic, IDiagnosticLocation? location ) => this._sink.Report( diagnostic, location, this._source );

    /// <inheritdoc />
    public void Suppress( SuppressionDefinition suppression, IDeclaration scope ) => this._sink.Suppress( suppression, scope, this._source );

    /// <inheritdoc />
    public void Suggest( CodeFix codeFix, IDiagnosticLocation location ) => this._sink.Suggest( codeFix, location, this._source );
}