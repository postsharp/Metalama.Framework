// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using System;
using System.ComponentModel;

namespace Metalama.Framework.Validation;

/// <summary>
/// Encapsulates an <see cref="IDiagnosticSink"/> and the default target of diagnostics, suppressions, and code fixes.
/// </summary>
/// <seealso href="@diagnostics"/>
[PublicAPI]
[CompileTime]
public readonly struct ReferenceValidationDiagnosticSink
{
    internal IDiagnosticSink Sink { get; }

    private readonly ReferenceValidationContext _context;

    internal ReferenceValidationDiagnosticSink(
        IDiagnosticSink sink,
        ReferenceValidationContext context )
    {
        this._context = context;
        this.Sink = sink;
    }

    /// <summary>
    /// Reports a diagnostic to the default location of the current <see cref="ScopedDiagnosticSink"/>..
    /// </summary>
    public void Report( IDiagnostic diagnostic )
    {
        foreach ( var scope in this._context.References )
        {
            this.Sink.Report( diagnostic, this._context.ResolveLocation( scope ), this._context.DiagnosticSource );
        }
    }

    public void Report( Func<ReferenceInstance, IDiagnostic?> getDiagnostic, Func<ReferenceInstance,IDiagnosticLocation?>? getLocation = null )
    {
        foreach ( var scope in this._context.References )
        {
            var diagnostic = getDiagnostic( scope );

            if ( diagnostic == null )
            {
                continue;
            }

            var location = getLocation?.Invoke( scope ) ?? this._context.ResolveLocation( scope );

            this.Sink.Report( diagnostic, location, this._context.DiagnosticSource );
        }
    }

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public void Suppress( SuppressionDefinition suppression ) => this.Suppress( (ISuppression) suppression );

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    public void Suppress( ISuppression suppression )
    {
        foreach ( var scope in this._context.References )
        {
            this.Sink.Suppress( suppression, this._context.ResolveDeclaration( scope ), this._context.DiagnosticSource );
        }
    }

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    public void Suggest( CodeFix codeFix )
    {
        foreach ( var scope in this._context.References )
        {
            this.Sink.Suggest(
                codeFix,
                this._context.ResolveDeclaration( scope ),
                this._context.DiagnosticSource );
        }
    }

    /// <summary>
    /// Reports a parametric diagnostic by specifying its location.
    /// </summary>
    public void Report( IDiagnostic diagnostic, IDiagnosticLocation location ) => this.Sink.Report( diagnostic, location, this._context.DiagnosticSource );

    /// <summary>
    /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
    /// </summary>
    /// <param name="suppression">The suppression definition, which must be defined as a static field or property.</param>
    /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public void Suppress( SuppressionDefinition suppression, IDeclaration scope ) => this.Suppress( (ISuppression) suppression, scope );

    /// <summary>
    /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
    /// </summary>
    /// <param name="suppression">The suppression definition, which must be defined as a static field or property.</param>
    /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
    public void Suppress( ISuppression suppression, IDeclaration scope ) => this.Sink.Suppress( suppression, scope, this._context.DiagnosticSource );

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    /// <param name="location">The code location for which the code fix should be suggested, typically an <see cref="IDeclaration"/>.</param>
    public void Suggest( CodeFix codeFix, IDiagnosticLocation location ) => this.Sink.Suggest( codeFix, location, this._context.DiagnosticSource );
}