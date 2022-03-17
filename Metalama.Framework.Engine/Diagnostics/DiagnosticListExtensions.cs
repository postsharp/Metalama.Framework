// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Diagnostics
{
    internal static class DiagnosticListExtensions
    {
        public static bool HasErrors( this IReadOnlyList<Diagnostic> diagnosticList ) => diagnosticList.Any( d => d.Severity == DiagnosticSeverity.Error );
    }
}