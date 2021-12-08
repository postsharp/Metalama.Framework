// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Diagnostics
{
    public static class DiagnosticAdderExtensions
    {
        public static void Report( this IDiagnosticAdder adder, IEnumerable<Diagnostic> diagnostics )
        {
            foreach ( var d in diagnostics )
            {
                adder.Report( d );
            }
        }
    }
}