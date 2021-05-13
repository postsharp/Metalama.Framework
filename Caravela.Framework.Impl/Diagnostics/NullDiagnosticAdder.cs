// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class NullDiagnosticAdder : IDiagnosticAdder
    {
        public static NullDiagnosticAdder Instance { get; } = new();

        private NullDiagnosticAdder() { }

        public void Report( Diagnostic diagnostic ) { }
    }
}