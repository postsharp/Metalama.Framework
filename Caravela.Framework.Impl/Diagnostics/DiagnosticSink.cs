// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public IDiagnosticScope? DefaultScope { get; private set; }

        protected DiagnosticSink( IDiagnosticScope? defaultScope )
        {
            this.DefaultScope = defaultScope;
        }

        public int ErrorCount { get; private set; }

        /// <summary>
        /// Reports a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        public abstract void ReportDiagnostic( Diagnostic diagnostic );

        public abstract void SuppressDiagnostic( string id, Location location );

        private static RoslynDiagnosticSeverity MapSeverity( Severity severity ) =>
            severity switch
            {
                Severity.Error => RoslynDiagnosticSeverity.Error,
                Severity.Hidden => RoslynDiagnosticSeverity.Hidden,
                Severity.Info => RoslynDiagnosticSeverity.Info,
                Severity.Warning => RoslynDiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };

        private IDiagnosticLocation? DefaultReportLocation => this.DefaultScope?.LocationForDiagnosticReport;

        private IEnumerable<IDiagnosticLocation> DefaultSuppressLocations 
            => this.DefaultScope?.LocationsForDiagnosticSuppression ?? Enumerable.Empty<IDiagnosticLocation>();

        public IDisposable WithDefaultScope( IDiagnosticScope scope )
        {
            var oldScope = this.DefaultScope;
            this.DefaultScope = scope;
            return new RestoreLocationCookie( this, oldScope );
        }

        public void ReportDiagnostic( Severity severity, IDiagnosticLocation? location, string id, string formatMessage, params object[] args )
        {
            var roslynLocation = ((DiagnosticLocation?) (location ?? this.DefaultReportLocation))?.Location;
            var roslynSeverity = MapSeverity( severity );
            var warningLevel = severity == Severity.Error ? 0 : 1;

            var diagnostic = Diagnostic.Create( 
                id,
                "Caravela.User",
                new NonLocalizableString( formatMessage, args ),
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
            this.ReportDiagnostic( severity, this.DefaultReportLocation, id, formatMessage, args );
        }

        public void SuppressDiagnostic( string id, IDiagnosticLocation? location = null )
        {
            void Suppress(IDiagnosticLocation defaultLocation)
            {
                var roslynLocation = ((DiagnosticLocation) defaultLocation).Location;
                if (roslynLocation != null)
                {
                    this.SuppressDiagnostic(id, roslynLocation);
                }
            }

            if ( location == null )
            {
                foreach ( var defaultLocation in this.DefaultSuppressLocations )
                {
                    if ( defaultLocation != null )
                    {
                        Suppress(defaultLocation);
                    }
                    else
                    {
                        // It should never be null but, if it is by mistake, it's better to avoid an infinite recursion.
                    }
                }
            }
            else
            {
                Suppress( location );
            }
        }
    }
}