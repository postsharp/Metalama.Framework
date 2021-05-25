// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class SeverityExtensions
    {
        public static DiagnosticSeverity ToRoslynSeverity( this Severity severity )
            => severity switch
            {
                Severity.Error => DiagnosticSeverity.Error,
                Severity.Hidden => DiagnosticSeverity.Hidden,
                Severity.Info => DiagnosticSeverity.Info,
                Severity.Warning => DiagnosticSeverity.Warning,
                _ => throw new AssertionFailedException()
            };
    }
}