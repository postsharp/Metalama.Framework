// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Diagnostics
{
    public readonly struct ImmutableUserDiagnosticList
    {
        public static ImmutableUserDiagnosticList Empty { get; } = new(
            ImmutableArray<Diagnostic>.Empty,
            ImmutableArray<ScopedSuppression>.Empty,
            ImmutableArray<CodeFixInstance>.Empty );

        public ImmutableArray<Diagnostic> ReportedDiagnostics { get; }

        public ImmutableArray<ScopedSuppression> DiagnosticSuppressions { get; }

        public ImmutableArray<CodeFixInstance> CodeFixes { get; }

        private ImmutableUserDiagnosticList(
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<ScopedSuppression> suppressions,
            ImmutableArray<CodeFixInstance> codeFixes )
        {
            this.CodeFixes = codeFixes.IsDefault ? ImmutableArray<CodeFixInstance>.Empty : codeFixes;
            this.ReportedDiagnostics = diagnostics.IsDefault ? ImmutableArray<Diagnostic>.Empty : diagnostics;
            this.DiagnosticSuppressions = suppressions.IsDefault ? ImmutableArray<ScopedSuppression>.Empty : suppressions;
        }

        public ImmutableUserDiagnosticList(
            ImmutableArray<Diagnostic>? diagnostics,
            ImmutableArray<ScopedSuppression>? suppressions,
            ImmutableArray<CodeFixInstance>? codeFixes )
            : this(
                diagnostics ?? ImmutableArray<Diagnostic>.Empty,
                suppressions ?? ImmutableArray<ScopedSuppression>.Empty,
                codeFixes ?? ImmutableArray<CodeFixInstance>.Empty ) { }

        // Coverage: ignore (design time)
        internal ImmutableUserDiagnosticList(
            IReadOnlyList<Diagnostic> diagnostics,
            ImmutableArray<ScopedSuppression> suppressions = default,
            ImmutableArray<CodeFixInstance> codeFixes = default )
            : this( diagnostics.ToImmutableArray(), suppressions, codeFixes ) { }

        internal ImmutableUserDiagnosticList Concat( in ImmutableUserDiagnosticList other )
            => new(
                this.ReportedDiagnostics.AddRange( other.ReportedDiagnostics ),
                this.DiagnosticSuppressions.AddRange( other.DiagnosticSuppressions ),
                this.CodeFixes.AddRange( other.CodeFixes ) );

        public override string ToString()
            => $"Diagnostics={this.ReportedDiagnostics.Length}, Suppressions={this.DiagnosticSuppressions.Length}, CodeFixes={this.CodeFixes.Length}";
    }
}