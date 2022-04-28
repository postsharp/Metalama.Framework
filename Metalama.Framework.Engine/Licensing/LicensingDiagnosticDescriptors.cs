// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Licensing;

internal static class LicensingDiagnosticDescriptors
{
    // Reserved range: 700-799

    private const string _category = "Metalama.General";

    internal static readonly DiagnosticDefinition<(int ActualCount, int MaxCount, string ClassNames)> TooManyFreemiumAspects =
        new(
            "LAMA00700",
            _category,
            "There are too {0} freemium aspects in the project, but only {1} are allowed. The freemium aspects are: {2}. " +
            " Please report this issue at https://www.postsharp.net/support and attach this file to the ticket.",
            Severity.Warning,
            "Too many freemium aspects in the project." );
}