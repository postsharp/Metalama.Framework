// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using System;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Encapsulates an <see cref="IDiagnosticSink"/> and the default target of diagnostics, suppressions, and code fixes.
/// </summary>
/// <seealso href="@diagnostics"/>
[PublicAPI]
[CompileTime]
public readonly struct ScopedDiagnosticSink
{
    internal IDiagnosticSink Sink { get; }

    private readonly IDiagnosticSource _source;

    /// <summary>
    /// Gets the declaration on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDeclaration? DefaultTargetDeclaration { get; }

    /// <summary>
    /// Gets the location on which diagnostics or code fixes will be reported or suppressed.
    /// </summary>
    public IDiagnosticLocation? DefaultTargetLocation { get; }

    internal ScopedDiagnosticSink(
        IDiagnosticSink sink,
        IDiagnosticSource source,
        IDiagnosticLocation? defaultTargetLocation,
        IDeclaration? defaultTargetDeclaration )
    {
        this.Sink = sink;
        this._source = source;
        this.DefaultTargetLocation = defaultTargetLocation;
        this.DefaultTargetDeclaration = defaultTargetDeclaration;
    }

    /// <summary>
    /// Reports a diagnostic to the default location of the current <see cref="ScopedDiagnosticSink"/>..
    /// </summary>
    /// <param name="diagnostic"></param>
    public void Report( IDiagnostic diagnostic ) => this.Sink.Report( diagnostic, this.DefaultTargetLocation, this._source );

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    /// <param name="suppression"></param>
    public void Suppress( SuppressionDefinition suppression )
    {
        this.Sink.Suppress(
            suppression,
            this.DefaultTargetDeclaration ?? throw new InvalidOperationException( "Use the overload that receives a scope declaration." ),
            this._source );
    }

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    public void Suggest( CodeFix codeFix )
        => this.Sink.Suggest(
            codeFix,
            this.DefaultTargetDeclaration ?? throw new InvalidOperationException( "Use the overload that receives a scope declaration." ),
            this._source );

    /// <summary>
    /// Reports a parametric diagnostic by specifying its location.
    /// </summary>
    public void Report( IDiagnostic diagnostic, IDiagnosticLocation? location ) => this.Sink.Report( diagnostic, location, this._source );

    /// <summary>
    /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
    /// </summary>
    /// <param name="suppression">The suppression definition, which must be defined as a static field or property.</param>
    /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
    public void Suppress( SuppressionDefinition suppression, IDeclaration scope ) => this.Sink.Suppress( suppression, scope, this._source );

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    /// <param name="location">The code location for which the code fix should be suggested, typically an <see cref="IDeclaration"/>.</param>
    public void Suggest( CodeFix codeFix, IDiagnosticLocation location ) => this.Sink.Suggest( codeFix, location, this._source );
}