// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Licensing;

internal static class LicensingDiagnosticDescriptors
{
    // Reserved range: 800-801

    private const string _category = "Metalama.General";

    internal static readonly DiagnosticDefinition<(int ActualCount, int MaxCount, string ClassNames)> TooManyAspectClasses =
        new(
            "LAMA00700",
            _category,
            "You have {0} aspect classes in the project exceed but only {1} are allowed by your license. The aspect classes are: {2}. ",
            Severity.Warning,
            "Too many aspect classes in the project." );
}