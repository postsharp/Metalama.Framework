// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

internal sealed class DiagnosticImpl<T> : IDiagnostic where T : notnull
{
    public DiagnosticDefinition<T> Definition { get; }

    public T Arguments { get; }

    object? IDiagnostic.Arguments => this.Arguments;

    IDiagnosticDefinition IDiagnostic.Definition => this.Definition;

    public ImmutableArray<CodeFix> CodeFixes { get; private set; }

    public DiagnosticImpl( DiagnosticDefinition<T> definition, T arguments, ImmutableArray<CodeFix> codeFixes )
    {
        this.Definition = definition;
        this.Arguments = arguments;
        this.CodeFixes = codeFixes;
    }

    public IDiagnostic WithCodeFixes( params CodeFix[] codeFixes )
    {
        this.CodeFixes = this.CodeFixes.AddRange( codeFixes );

        return this;
    }

    public void ReportTo( IDiagnosticLocation location, IDiagnosticSink sink ) => sink.Report( location, this );

    public void ReportTo( in ScopedDiagnosticSink sink ) => sink.Report( this );
}