// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A list of <see cref="Diagnostic"/> that implements <see cref="IDiagnosticSink"/>.
    /// </summary>
    public class DiagnosticListBuilder : DiagnosticSink
    {
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;

        internal DiagnosticListBuilder( ICodeElement? defaultScope = null )
            : base( defaultScope ) { }

        /// <inheritdoc/>
        public override void ReportDiagnostic( Diagnostic diagnostic )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.Add( diagnostic );
        }

        public override void SuppressDiagnostic( ScopedSuppression suppression )
        {
            this._suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
            this._suppressions.Add( suppression );
        }

        public ImmutableDiagnosticList ToImmutable()
            => new(
                this._diagnostics?.ToImmutable() ?? ImmutableArray<Diagnostic>.Empty,
                this._suppressions?.ToImmutable() ?? ImmutableArray<ScopedSuppression>.Empty );
    }
}