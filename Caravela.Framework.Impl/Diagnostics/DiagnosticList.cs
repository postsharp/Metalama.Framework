// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A list of <see cref="Diagnostic"/> that implements <see cref="IDiagnosticSink"/>.
    /// </summary>
    public class DiagnosticList : DiagnosticSink
    {

        private List<Diagnostic>? _diagnostics;

        public DiagnosticList( IDiagnosticLocation? defaultLocation = null ) : base( defaultLocation )
        {
        }

        /// <inheritdoc/>
        public override void ReportDiagnostic( Diagnostic diagnostic )
        {
            this._diagnostics ??= new List<Diagnostic>();
            this._diagnostics.Add( diagnostic );
        }

        public IReadOnlyList<Diagnostic> Diagnostics => (IReadOnlyList<Diagnostic>?) this._diagnostics ?? Array.Empty<Diagnostic>();
    }
}