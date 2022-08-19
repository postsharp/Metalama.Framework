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
        var remainingAspectClasses = new List<IAspectClass>();

        foreach ( var aspectInstanceResult in aspectInstanceResults )
        {
            if ( aspectInstanceResult.AspectInstance.AspectClass is AspectClass aspectClass
                && aspectClass.Project != null
                && !string.IsNullOrEmpty( aspectClass.Project.ProjectLicenseInfo.RedistributionLicenseKey ) )
            {
                if ( !redistributionAspectClassesPerProject.TryGetValue( aspectClass.Project, out var aspects ) )
                {
                    aspects = new List<AspectClass>();
                    redistributionAspectClassesPerProject.Add( aspectClass.Project, aspects );
                }

                aspects.Add( aspectClass );
            }
            else
            {
                remainingAspectClasses.Add( aspectInstanceResult.AspectInstance.AspectClass );
            }
        }

        // TODO: Verify the redistribution license keys.

        var aspectClassesCount = redistributionAspectClassesPerProject.Count + remainingAspectClasses.Count;
        var maxAspectsCount = this._licenseConsumptionManager.GetMaxAspectsCount();

        if ( aspectClassesCount > maxAspectsCount )
        {
            // TODO: list
            // var aspectClassNames = string.Join( ",", aspectClasses.Select( x => "'" + x.ShortName + "'" ) );
            var aspectClassNames = "TODO";

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