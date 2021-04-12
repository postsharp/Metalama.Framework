// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    public readonly struct ImmutableDiagnosticList
    {
        public static ImmutableDiagnosticList Empty { get; } = new ImmutableDiagnosticList( ImmutableArray<Diagnostic>.Empty, ImmutableArray<ScopedSuppression>.Empty );

        public ImmutableArray<Diagnostic> ReportedDiagnostics { get; }

        public ImmutableArray<ScopedSuppression> DiagnosticSuppressions { get; }

        public ImmutableDiagnosticList( ImmutableArray<Diagnostic> diagnostics, ImmutableArray<ScopedSuppression> suppressions )
        {
            this.ReportedDiagnostics = diagnostics;
            this.DiagnosticSuppressions = suppressions;
        }

        public ImmutableDiagnosticList Concat( in ImmutableDiagnosticList other )
            => new ImmutableDiagnosticList(
                this.ReportedDiagnostics.AddRange( other.ReportedDiagnostics ),
                this.DiagnosticSuppressions.AddRange( other.DiagnosticSuppressions ) );
    }
}