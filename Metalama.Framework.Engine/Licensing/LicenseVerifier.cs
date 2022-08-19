// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Licensing;

#pragma warning disable SA1118

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
internal class LicenseVerifier : IService
{
    // TODO: Namespace-limited licenses.

    private readonly ILicenseConsumptionManager _licenseConsumptionManager;

    public LicenseVerifier( ILicenseConsumptionManager licenseConsumptionManager )
    {
        this._licenseConsumptionManager = licenseConsumptionManager;
    }

    public void VerifyCanAddChildAspect( AspectPredecessor predecessor )
    {
        if ( !this._licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.MetalamaFabricsAspects ) )
        {
            switch ( predecessor.Instance )
            {
                case IFabricInstance fabricInstance:
                    throw new DiagnosticException(
                        LicensingDiagnosticDescriptors.FabricsNotAvailable.CreateRoslynDiagnostic(
                            null,
                            (fabricInstance.Fabric.GetType().Name, "add an aspect") ) );
            }
        }
    }

    private static bool HasRedistributionLicense( CompileTimeProject? project ) => project?.ProjectLicenseInfo.RedistributionLicenseKey != null;

    public void VerifyCanValidator( AspectPredecessor predecessor )
    {
        if ( !this._licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.MetalamaFabricsValidators ) )
        {
            switch ( predecessor.Instance )
            {
                case IFabricInstance fabricInstance:
                    throw new DiagnosticException(
                        LicensingDiagnosticDescriptors.FabricsNotAvailable.CreateRoslynDiagnostic(
                            null,
                            (fabricInstance.Fabric.GetType().Name, "add a validator") ) );
            }
        }
    }

    public void VerifyCompilationResult( ImmutableArray<AspectInstanceResult> aspectInstanceResults, UserDiagnosticSink diagnostics )
    {
        var redistributionAspectClassesPerProject = new Dictionary<CompileTimeProject, List<AspectClass>>();
        var otherAspectClasses = new List<IAspectClass>();
        var invalidRedistributionLicenseKeys = new HashSet<string>();

        foreach ( var aspectInstanceResult in aspectInstanceResults )
        {
            if ( aspectInstanceResult.AspectInstance.AspectClass is AspectClass aspectClass
                && aspectClass.Project != null
                && !string.IsNullOrEmpty( aspectClass.Project.ProjectLicenseInfo.RedistributionLicenseKey ) )
            {
                var projectRedistributionLicenseKey = aspectClass.Project.ProjectLicenseInfo.RedistributionLicenseKey!;

                if ( invalidRedistributionLicenseKeys.Contains( projectRedistributionLicenseKey ) )
                {
                    continue;
                }

                if ( !this._licenseConsumptionManager.ValidateRedistributionLicenseKey( projectRedistributionLicenseKey ) )
                {
                    invalidRedistributionLicenseKeys.Add( projectRedistributionLicenseKey );

                    diagnostics.Report(
                        LicensingDiagnosticDescriptors.RedistributionLicenseInvalid.CreateRoslynDiagnostic(
                            null, aspectClass.Project.RunTimeIdentity.Name ) );

                    continue;
                }

                if ( !redistributionAspectClassesPerProject.TryGetValue( aspectClass.Project, out var aspects ) )
                {
                    aspects = new List<AspectClass>();
                    redistributionAspectClassesPerProject.Add( aspectClass.Project, aspects );
                }

                aspects.Add( aspectClass );
            }
            else
            {
                otherAspectClasses.Add( aspectInstanceResult.AspectInstance.AspectClass );
            }
        }

        var aspectClassesCount = redistributionAspectClassesPerProject.Count + otherAspectClasses.Count;
        var maxAspectsCount = this._licenseConsumptionManager.GetMaxAspectsCount();

        if ( aspectClassesCount > maxAspectsCount )
        {
            static string GetNames( IEnumerable<IAspectClass> aspectClasses ) => string.Join( ", ", aspectClasses.Select( a => $"'{a.ShortName}'" ) );

            // This is to make test output deterministic.
            static string NormalizeAssemblyName( string assemblyName ) => Regex.IsMatch( assemblyName, "^dependency_[0-9a-f]{16}$" )
                ? "dependency_XXXXXXXXXXXXXXXX"
                : assemblyName;

            var aspectClassNames = string.Join( ", ", GetNames( otherAspectClasses ) );

            if (redistributionAspectClassesPerProject.Count > 0)
            {
                aspectClassNames += ", ";
                aspectClassNames += string.Join( ", ", redistributionAspectClassesPerProject.Select(
                    r => $"aspects from '{NormalizeAssemblyName( r.Key.RunTimeIdentity.Name )}' assembly counted as one ({GetNames( r.Value )})" ) );
            }

            diagnostics.Report(
                LicensingDiagnosticDescriptors.TooManyAspectClasses.CreateRoslynDiagnostic(
                    null,
                    (aspectClassesCount, maxAspectsCount, aspectClassNames) ) );
        }
    }

    public void VerifyCanBeInherited( AspectClass aspectClass, IAspect? prototype, IDiagnosticAdder diagnostics )
    {
        if ( prototype == null )
        {
            // This happens only with abstract classes.
            return;
        }

        if ( aspectClass.IsInherited && !this._licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.MetalamaAspectInheritance ) )
        {
            diagnostics.Report(
                LicensingDiagnosticDescriptors.InheritanceNotAvailable.CreateRoslynDiagnostic(
                    null, aspectClass.ShortName ) );
        }
    }
}