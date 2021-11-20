// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Diagnostics
{
    public readonly struct ImmutableUserDiagnosticList
    {
        public static ImmutableUserDiagnosticList Empty { get; } = new( ImmutableArray<Diagnostic>.Empty, ImmutableArray<ScopedSuppression>.Empty );

        public ImmutableArray<Diagnostic> ReportedDiagnostics { get; }

        public ImmutableArray<ScopedSuppression> DiagnosticSuppressions { get; }

        public ImmutableArray<CodeFixInstance> CodeFixes { get; }

        public ImmutableUserDiagnosticList( ImmutableArray<Diagnostic>? diagnostics, ImmutableArray<ScopedSuppression>? suppressions )
        {
            this.ReportedDiagnostics = diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            this.DiagnosticSuppressions = suppressions ?? ImmutableArray<ScopedSuppression>.Empty;
        }

        // Coverage: ignore (design time)
        internal ImmutableUserDiagnosticList( DiagnosticList diagnosticList ) : this( diagnosticList.ToImmutableArray(), null ) { }

        public ImmutableUserDiagnosticList Concat( in ImmutableUserDiagnosticList other )
            => new(
                this.ReportedDiagnostics.AddRange( other.ReportedDiagnostics ),
                this.DiagnosticSuppressions.AddRange( other.DiagnosticSuppressions ) );

        public override string ToString() => $"Diagnostics={this.ReportedDiagnostics.Length}, Suppressions={this.DiagnosticSuppressions.Length}";
    }
}