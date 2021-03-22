// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A list of <see cref="Diagnostic"/> that implements <see cref="IDiagnosticSink"/>.
    /// </summary>
    public class DiagnosticListBuilder : DiagnosticSink
    {

        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;

        internal DiagnosticListBuilder( IDiagnosticScope? defaultScope = null )
            : base( defaultScope )
        {
        }

        /// <inheritdoc/>
        public override void ReportDiagnostic( Diagnostic diagnostic )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.Add( diagnostic );
        }

        public void ReportDiagnostics( IEnumerable<Diagnostic> diagnostics )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.AddRange( diagnostics );
        }

        public override void SuppressDiagnostic( string id, Location location )
        {
            this._suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
            this._suppressions.Add( new ScopedSuppression(id, location) );
        }

        public void SuppressDiagnostics( IEnumerable<ScopedSuppression> suppressions )
        {
            this._suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
            this._suppressions.AddRange( suppressions );
        }

        public DiagnosticList ToDiagnosticList()
            => new DiagnosticList( this._diagnostics?.ToImmutable(), this._suppressions?.ToImmutable() );
    }
}