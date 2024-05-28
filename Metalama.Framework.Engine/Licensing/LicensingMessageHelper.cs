// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Licensing;

internal static class LicensingMessageHelper
{
    public static void Report( this ImmutableArray<LicensingMessage> messages, Action<Diagnostic>? adder )
    {
        if ( adder == null )
        {
            return;
        }

        foreach ( var message in messages )
        {
            var diagnosticDefinition = message.IsError
                ? LicensingDiagnosticDescriptors.LicensingError
                : LicensingDiagnosticDescriptors.LicensingWarning;

            adder( diagnosticDefinition.CreateRoslynDiagnostic( null, message.Text ) );
        }
    }
}