// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics
{
    public static class SeverityExtensions
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

        public static Severity ToOurSeverity( this DiagnosticSeverity severity )
            => severity switch
            {
                DiagnosticSeverity.Error => Severity.Error,
                DiagnosticSeverity.Hidden => Severity.Hidden,
                DiagnosticSeverity.Info => Severity.Info,
                DiagnosticSeverity.Warning => Severity.Warning,
                _ => throw new AssertionFailedException()
            };
    }
}