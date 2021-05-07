// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RoslynDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    public partial class DiagnosticSink : IDiagnosticSink, IDiagnosticAdder
    {
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;

        public ICodeElement? DefaultScope { get; private set; }

        public DiagnosticSink( ICodeElement? defaultScope = null )
        {
            this.DefaultScope = defaultScope;
        }

        public int ErrorCount { get; private set; }

        public void Report( Diagnostic diagnostic )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.Add( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.ErrorCount++;
            }
        }

        public void Suppress( ScopedSuppression suppression )
        {
            this._suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
            this._suppressions.Add( suppression );
        }

        public void Suppress( string id, ICodeElement scope ) => this.Suppress( new ScopedSuppression( id, scope ) );

        public void Suppress( IEnumerable<ScopedSuppression> suppressions )
        {
            foreach ( var suppression in suppressions )
            {
                this.Suppress( suppression );
            }
        }

        public void Suppress( string id )
        {
            if ( this.DefaultScope != null )
            {
                this.Suppress( new ScopedSuppression( id, this.DefaultScope ) );
            }
        }

        private static RoslynDiagnosticSeverity MapSeverity( Severity severity )
            => severity switch
            {
                Severity.Error => RoslynDiagnosticSeverity.Error,
                Severity.Hidden => RoslynDiagnosticSeverity.Hidden,
                Severity.Info => RoslynDiagnosticSeverity.Info,
                Severity.Warning => RoslynDiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };

        public IDisposable WithDefaultScope( ICodeElement scope )
        {
            var oldScope = this.DefaultScope;
            this.DefaultScope = scope;

            return new RestoreLocationCookie( this, oldScope );
        }

        public void Report( Severity severity, IDiagnosticLocation location, string id, string formatMessage, params object[] args )
        {
            var roslynLocation = ((DiagnosticLocation) location).Location ?? this.DefaultScope?.GetDiagnosticLocation();
            var roslynSeverity = MapSeverity( severity );
            var warningLevel = severity == Severity.Error ? 0 : 1;

            var diagnostic = Diagnostic.Create(
                id,
                "Caravela.User",
                new NonLocalizedString( formatMessage, args ),
                roslynSeverity,
                roslynSeverity,
                true,
                warningLevel,
                false,
                location: roslynLocation );

            this.Report( diagnostic );

            if ( severity == Severity.Error )
            {
                this.ErrorCount++;
            }
        }

        public void Report( Severity severity, string id, string formatMessage, params object[] args )
        {
            if ( this.DefaultScope == null )
            {
                throw new InvalidOperationException( "Cannot report a diagnostic when the default scope has not been defined." );
            }

            this.Report( severity, this.DefaultScope, id, formatMessage, args );
        }

        public ImmutableDiagnosticList ToImmutable()
            => new(
                this._diagnostics?.ToImmutable() ?? ImmutableArray<Diagnostic>.Empty,
                this._suppressions?.ToImmutable() ?? ImmutableArray<ScopedSuppression>.Empty );

        public override string ToString() => $"Diagnostics={this._diagnostics?.Count ?? 0}, Suppressions={this._suppressions?.Count ?? 0}";
    }
}