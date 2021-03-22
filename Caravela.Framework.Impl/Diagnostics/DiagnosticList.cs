// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    public class DiagnosticList
    {
        public static DiagnosticList Empty { get; } = new DiagnosticList( null, null );

        public ImmutableArray<Diagnostic> ReportedDiagnostics { get; }

        public ImmutableArray<ScopedSuppression> DiagnosticSuppressions { get; }

        public DiagnosticList( ImmutableArray<Diagnostic>? diagnostics, ImmutableArray<ScopedSuppression>? suppressions )
        {
            this.ReportedDiagnostics = diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            this.DiagnosticSuppressions = suppressions ?? ImmutableArray<ScopedSuppression>.Empty;
        }

        public DiagnosticList Concat( DiagnosticList other )
            => new DiagnosticList( 
                this.ReportedDiagnostics.AddRange( other.ReportedDiagnostics ),
                this.DiagnosticSuppressions.AddRange( other.DiagnosticSuppressions ) );
    }
}