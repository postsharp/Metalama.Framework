// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Engine.Licensing;

public static class LicensingDiagnosticDescriptors
{
    internal const string InvalidLicenseKeyRegisteredId = "LAMA0812";
    internal const string NoLicenseKeyRegisteredId = "LAMA0809";
    internal const string RedistributionLicenseInvalidId = "LAMA0803";
    internal const string TooManyAspectClassesId = "LAMA0800";
    internal const string FabricsNotAvailableId = "LAMA0801";

    // Reserved range: 800-819

    private const string _category = "Metalama.Licensing";

    internal static readonly DiagnosticDefinition<(int ActualCount, int MaxAspectsCount, string ProjectName)> TooManyAspectClasses =
        new(
            TooManyAspectClassesId,
            _category,
            "This project uses {0} aspect classes, but only {1} are allowed by your license. For details, use the following command: `metalama license usage details --project {2}`.",
            Severity.Error,
            "Too many aspect classes in the project." );

    internal static readonly DiagnosticDefinition<(string FabricName, string Feature)> FabricsNotAvailable =
        new(
            FabricsNotAvailableId,
            _category,
            "The '{0}' fabric cannot {1} because this feature is not covered by your license. You can only {1} from an aspect.",
            Severity.Error,
            "Cannot {1} using fabrics." );

    internal static readonly DiagnosticDefinition<string> RedistributionLicenseInvalid =
        new(
            RedistributionLicenseInvalidId,
            _category,
            "The redistribution license of '{0}' assembly is invalid.",
            Severity.Error,
            "Invalid redistribution license of '{0}' assembly." );

    internal static readonly DiagnosticDefinition<(string Title, string Origin)> CodeActionNotAvailable =
        new(
            "LAMA0805",
            Severity.Error,
            "The '{0}' code action provided by '{1}' cannot be applied because code actions are not covered by your license.",
            "Code actions not available",
            _category );

    internal static readonly DiagnosticDefinition<string> LicensingWarning = new(
        "LAMA0806",
        Severity.Warning,
        "{0}",
        "Licensing warning.",
        _category );

    internal static readonly DiagnosticDefinition<string> LicensingError = new(
        "LAMA0807",
        Severity.Error,
        "{0}",
        "Licensing error.",
        _category );

    internal static readonly DiagnosticDefinition NoLicenseKeyRegistered
        = new(
            NoLicenseKeyRegisteredId,
            Severity.Error,
            "You must activate Metalama or register a license key before building your project. See https://postsharp.net/links/metalama-register-license.",
            "You must activate Metalama or register a license key before building your project.",
            _category );

    internal static readonly DiagnosticDefinition RoslynApiNotAvailable =
        new(
            "LAMA0810",
            Severity.Error,
            "Accessing the Roslyn API via Metalama.Framework.Sdk package is not covered by your license.",
            "Roslyn API not available.",
            _category );

    internal static readonly DiagnosticDefinition<string> InvalidLicenseKeyRegistered
        = new(
            InvalidLicenseKeyRegisteredId,
            Severity.Error,
            "The registered license key '{0}' is not valid for Metalama or for this project.",
            "The registered license key is not valid for Metalama or for this project.",
            _category );
}