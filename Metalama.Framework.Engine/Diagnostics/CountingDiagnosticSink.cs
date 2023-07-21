// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Diagnostics;

/// <summary>
/// A wrapping implementation of <see cref="IDiagnosticSink"/> that counts the reported diagnostics.
/// </summary>
internal sealed class CountingDiagnosticSink : IDiagnosticSink
{
    private readonly IDiagnosticSink _underlying;

    public CountingDiagnosticSink( IDiagnosticSink underlying )
    {
        this._underlying = underlying;
    }

    public int DiagnosticCount { get; private set; }

    public void Report( IDiagnostic diagnostic, IDiagnosticLocation? location, IDiagnosticSource source )
    {
        this.DiagnosticCount++;
        this._underlying.Report( diagnostic, location, source );
    }

    public void Suppress( SuppressionDefinition suppression, IDeclaration scope, IDiagnosticSource source )
        => this._underlying.Suppress( suppression, scope, source );

    public void Suggest( CodeFix codeFix, IDiagnosticLocation location, IDiagnosticSource source ) => this._underlying.Suggest( codeFix, location, source );
}