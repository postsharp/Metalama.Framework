// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics
{
    // ReSharper disable once UnusedTypeParameter

    public sealed class DiagnosticDefinition : DiagnosticDefinition<None>, IDiagnostic
    {
        public DiagnosticDefinition( string id, Severity severity, string messageFormat, string? title = null, string? category = null ) : base(
            id,
            severity,
            messageFormat,
            title,
            category ) { }

        IDiagnosticDefinition IDiagnostic.Definition => this;

        ImmutableArray<CodeFix> IDiagnostic.CodeFixes => ImmutableArray<CodeFix>.Empty;

        object? IDiagnostic.Arguments => default(None);

        public IDiagnostic WithCodeFixes( params CodeFix[] codeFixes ) => new DiagnosticImpl<None>( this, default, codeFixes.ToImmutableArray() );

        public void ReportTo( IDiagnosticLocation location, IDiagnosticSink sink ) => sink.Report( location, this );

        public void ReportTo( in ScopedDiagnosticSink sink ) => sink.Report( this );
    }
}