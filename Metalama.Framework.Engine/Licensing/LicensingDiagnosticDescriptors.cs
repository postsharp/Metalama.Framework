// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Licensing;

internal static class LicensingDiagnosticDescriptors
{
    // Reserved range: 800-802

    private const string _category = "Metalama.General";

    internal static readonly DiagnosticDefinition<(int ActualCount, int MaxCount, string ClassNames)> TooManyAspectClasses =
        new(
            "LAMA0800",
            _category,
            "You have {0} aspect classes used in the project but only {1} are allowed by your license. The aspect classes are: {2}.",
            Severity.Error,
            "Too many aspect classes in the project." );

    internal static readonly DiagnosticDefinition<(string FabricName, string Feature)> FabricsNotAvailable =
        new(
            "LAMA0801",
            _category,
            "The '{0}' fabric cannot {1} because this feature is not covered by Metalama Essentials license. You can only {1} validator from an aspect using Metalama Essentials.",
            Severity.Error,
            "Cannot {1} using fabrics with Metalama Essentials." );

    internal static readonly DiagnosticDefinition<string> InheritanceNotAvailable =
        new(
            "LAMA0802",
            _category,
            "The '{0}' aspect cannot be inherited because this feature is not covered by Metalama Essentials license.",
            Severity.Error,
            "Cannot inherit aspects with Metalama Essentials." );
}