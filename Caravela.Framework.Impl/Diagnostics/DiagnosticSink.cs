// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
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
        public ICodeElement? DefaultScope { get; private set; }

        protected DiagnosticSink( ICodeElement? defaultScope )
        {
            this.DefaultScope = defaultScope;
        }

        public int ErrorCount { get; private set; }

        /// <summary>
        /// Reports a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="diagnostic"></param>
        public abstract void ReportDiagnostic( Diagnostic diagnostic );

        public abstract void SuppressDiagnostic( string id, ICodeElement scope );
        
        public void SuppressDiagnostic( string id )
        {
            if ( this.DefaultScope != null )
            {
                this.SuppressDiagnostic( id, this.DefaultScope );
            }
        }

        private static RoslynDiagnosticSeverity MapSeverity( Severity severity ) =>
            severity switch
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

        public void ReportDiagnostic( Severity severity, IDiagnosticLocation location, string id, string formatMessage, params object[] args )
        {
            var roslynLocation = ((DiagnosticLocation) location).Location ?? this.DefaultScope?.GetLocationForDiagnosticReport();
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
            if ( this.DefaultScope == null )
            {
                throw new InvalidOperationException( "Cannot report a diagnostic when the default scope has not been defined." );
            }

            this.ReportDiagnostic( severity, this.DefaultScope, id, formatMessage, args );
        }
    }
}