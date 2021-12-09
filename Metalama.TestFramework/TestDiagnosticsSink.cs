// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using PostSharp.Backstage.Extensibility;
using System.Collections.Generic;

namespace Metalama.TestFramework
{
    internal class TestDiagnosticsSink : IBackstageDiagnosticSink, IService
    {
        private readonly List<(string Message, IDiagnosticsLocation? Location)> _warnings = new();
        private readonly List<(string Message, IDiagnosticsLocation? Location)> _errors = new();

        public void ReportWarning( string message, IDiagnosticsLocation? location = null ) => this._warnings.Add( (message, location) );

        public void ReportError( string message, IDiagnosticsLocation? location = null ) => this._errors.Add( (message, location) );

        public IEnumerable<Diagnostic> EnumerateDiagnostics()
        {
            foreach ( var warning in this._warnings )
            {
                yield return Diagnostic.Create(
                    "TEST",
                    "Metalama Test Framework",
                    new NonLocalizedString( warning.Message ),
                    DiagnosticSeverity.Warning,
                    DiagnosticSeverity.Warning,
                    true,
                    1 );
            }

            foreach ( var warning in this._errors )
            {
                yield return Diagnostic.Create(
                    "TEST",
                    "Metalama Test Framework",
                    new NonLocalizedString( warning.Message ),
                    DiagnosticSeverity.Error,
                    DiagnosticSeverity.Error,
                    true,
                    0 );
            }
        }
    }
}