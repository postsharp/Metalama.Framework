// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using RoslynDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    public abstract partial class DiagnosticSink : IDiagnosticSink
    {

        protected DiagnosticSink( IDiagnosticLocation? defaultLocation = null )
        {
            this.DefaultLocation = defaultLocation;
        }

        public int ErrorCount { get; private set; }

        /// <summary>
        /// Reports a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        protected abstract void ReportDiagnostic( Diagnostic diagnostic );

        private static RoslynDiagnosticSeverity MapSeverity( Severity severity )
        {
            return severity switch
            {
                Severity.Error => RoslynDiagnosticSeverity.Error,
                Severity.Hidden => RoslynDiagnosticSeverity.Hidden,
                Severity.Info => RoslynDiagnosticSeverity.Info,
                Severity.Warning => RoslynDiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };
        }

        public IDiagnosticLocation? DefaultLocation { get; private set; }

        public IDisposable WithDefaultLocation( IDiagnosticLocation? location )
        {
            var oldLocation = this.DefaultLocation;
            this.DefaultLocation = location;
            return new RestoreLocationCookie( this, oldLocation );
        }

        public void ReportDiagnostic( Severity severity, IDiagnosticLocation? location, string id, string formatMessage, params object[] args )
        {
            var roslynLocation = ((DiagnosticLocation?) (location ?? this.DefaultLocation))?.Location;
            var roslynSeverity = MapSeverity( severity );
            var warningLevel = severity == Severity.Error ? 0 : 1;

            var diagnostic = Diagnostic.Create( 
                id,
                "Caravela.User",
                new NonLocalizableString( formatMessage ),
                roslynSeverity,
                roslynSeverity,
                true,
                warningLevel,
                false,
                location: roslynLocation );

            this.ReportDiagnostic( diagnostic );

            if ( severity == Severity.Error )
            {
                this.ErrorCount++;
            }
        }

        public void ReportDiagnostic( Severity severity, string id, string formatMessage, params object[] args )
        {
            this.ReportDiagnostic( severity, this.DefaultLocation, id, formatMessage, args );
        }
    }
}