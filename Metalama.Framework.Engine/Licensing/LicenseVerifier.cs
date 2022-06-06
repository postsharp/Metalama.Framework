// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Licensing;

#pragma warning disable SA1118

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
internal class LicenseVerifier : IService
{
    private const int _maxAspectClasses = int.MaxValue;
    private readonly bool _isLimitedLicense;

    public LicenseVerifier( IServiceProvider serviceProvider )
    {
        var licenseConsumptionManager = (ILicenseConsumptionManager?) serviceProvider.GetService( typeof(ILicenseConsumptionManager) );

        if ( licenseConsumptionManager != null )
        {
            // TODO: if the _current_ project has a redistribution license, it has no limitation.
            this._isLimitedLicense = !licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.Metalama );
        }
        else
        {
            this._isLimitedLicense = false;
        }
    }

    public void VerifyCanAddChildAspect( AspectPredecessor predecessor )
    {
        // Adding children aspects is currently not limited.
    }

    private static bool HasRedistributionLicense( CompileTimeProject? project )
    {
        if ( project == null )
        {
            return false;
        }

        // TODO: project.LicenseKeys

        return false;
    }

    public void VerifyCanValidator( AspectPredecessor predecessor )
    {
        if ( this._isLimitedLicense )
        {
            switch ( predecessor.Instance )
            {
                case IFabricInstance fabricInstance:
                    throw new InvalidOperationException(
                        $"The '{fabricInstance.Fabric.GetType().Name}' fabric cannot add an aspect because this feature is not covered by Metalama Essentials license. You can add a only validator from an aspect using Metalama Essentials." );
            }
        }
    }

    public void VerifyCompilationResult( ImmutableArray<AspectInstanceResult> aspectInstanceResults, UserDiagnosticSink diagnostics )
    {
        var aspectClasses = aspectInstanceResults.Select( a => a.AspectInstance.AspectClass ).Distinct().ToList();

        if ( aspectClasses.Count > _maxAspectClasses )
        {
            var aspectClassNames = string.Join( ",", aspectClasses.Select( x => "'" + x.ShortName + "'" ) );

            diagnostics.Report(
                LicensingDiagnosticDescriptors.TooManyAspectClasses.CreateRoslynDiagnostic(
                    null,
                    (aspectClasses.Count, _maxAspectClasses, aspectClassNames) ) );
        }
    }

    public void VerifyCanBeInherited( AspectClass aspectClass, IAspect? prototype, IDiagnosticAdder diagnosticAdder )
    {
        if ( prototype == null )
        {
            // This happens only with abstract classes.
            return;
        }

        // Inheritance is currently unlimited.
    }
}