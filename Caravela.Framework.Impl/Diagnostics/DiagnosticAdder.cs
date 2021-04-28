// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class DiagnosticAdder : IDiagnosticAdder
    {
        private readonly Action<Diagnostic> _action;
        private int _errors; // For debugging only.

        public DiagnosticAdder( Action<Diagnostic> action )
        {
            this._action = action;
        }

        public void ReportDiagnostic( Diagnostic diagnostic )
        {
            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this._errors++;
            }
            
            this._action( diagnostic );
        }

        public override string ToString() => $"{this._errors} error(s)";
    }
}