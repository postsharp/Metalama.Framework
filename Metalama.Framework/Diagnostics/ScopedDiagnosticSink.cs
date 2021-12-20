using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Encapsulates an <see cref="IDiagnosticSink"/> and the default target of diagnostics, suppressions, and code fixes.
/// </summary>
public readonly struct ScopedDiagnosticSink : IDiagnosticSink
{
    private readonly IDiagnosticSink _sink;
    private readonly IDeclaration _declaration;
    private readonly IDiagnosticLocation _location;

    internal ScopedDiagnosticSink( IDiagnosticSink sink, IDiagnosticLocation location, IDeclaration declaration )
    {
        this._sink = sink;
        this._location = location;
        this._declaration = declaration;
    }

    /// <summary>
    /// Reports a diagnostic to the default location of the current <see cref="ScopedDiagnosticSink"/>..
    /// </summary>
    /// <param name="diagnostic"></param>
    public void Report( IDiagnostic diagnostic ) => this._sink.Report( this._location, diagnostic );

    /// <summary>
    /// Suppresses a diagnostic from the default declaration of the current <see cref="ScopedDiagnosticSink"/>.
    /// </summary>
    /// <param name="definition"></param>
    public void Suppress( SuppressionDefinition definition ) => this._sink.Suppress( this._declaration, definition );

    /// <summary>
    /// Suggest a code fix without reporting a diagnostic.
    /// </summary>
    /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
    public void Suggest( CodeFix codeFix ) => this._sink.Suggest( this._declaration, codeFix );

    /// <inheritdoc />
    public void Report( IDiagnosticLocation? location, IDiagnostic diagnostic ) => this._sink.Report( location, diagnostic );

    /// <inheritdoc />
    public void Suppress( IDeclaration scope, SuppressionDefinition definition ) => this._sink.Suppress( scope, definition );

    /// <inheritdoc />
    public void Suggest( IDiagnosticLocation location, CodeFix codeFix ) => this._sink.Suggest( location, codeFix );
}