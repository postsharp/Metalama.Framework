// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    public interface IDiagnosticAdder
    {
        void ReportDiagnostic( Diagnostic diagnostic );
    }

    internal class DiagnosticAdder : IDiagnosticAdder
    {
        private Action<Diagnostic> _action;

        public DiagnosticAdder( Action<Diagnostic> action ) {
            this._action = action;
        }

        public void ReportDiagnostic( Diagnostic diagnostic ) => this._action(diagnostic);
    }
}