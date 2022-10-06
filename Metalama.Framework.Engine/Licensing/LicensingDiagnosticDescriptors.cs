﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Licensing;

public static class LicensingDiagnosticDescriptors
{
    // Reserved range: 800-819

    private const string _category = "Metalama.General";

    internal static readonly DiagnosticDefinition<(int ActualCount, int MaxAspectsCount, string ClassNames)> TooManyAspectClasses =
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
            "The '{0}' fabric cannot {1} because this feature is not covered by your license. You can only {1} validator from an aspect.",
            Severity.Error,
            "Cannot {1} using fabrics." );

    internal static readonly DiagnosticDefinition<string> InheritanceNotAvailable =
        new(
            "LAMA0802",
            _category,
            "The '{0}' aspect cannot be inherited because this feature is not covered by your license.",
            Severity.Error,
            "Cannot inherit aspects." );

    internal static readonly DiagnosticDefinition<string> RedistributionLicenseInvalid =
        new(
            "LAMA0803",
            _category,
            "The redistribution license of '{0}' assembly is invalid.",
            Severity.Error,
            "Invalid redistribution license of '{0}' assembly." );

    internal static readonly DiagnosticDefinition<(string, string)> SdkNotAvailable =
        new(
            "LAMA0804",
            _category,
            "The '{0}' aspect weaver cannot be used to weave aspects as Metalama SDK is not covered by your license. The aspect classes are: {1}.",
            Severity.Error,
            "Metalama SDK not available." );

    public static readonly DiagnosticDefinition<(string, string)>
        CodeActionNotAvailable
            = new(
                "LAMA0805",
                Severity.Error,
                "The code action '{0}' provided by aspect '{1}' cannot be applied because code actions are not covered by your license.",
                "Code actions not available",
                _category );
}