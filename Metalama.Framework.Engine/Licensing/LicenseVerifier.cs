// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Licensing;

#pragma warning disable SA1118

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
internal class LicenseVerifier : IService
{
    private const int _maxFreemiumAspects = 1;
    private readonly bool _isLimitedLicense;

    public LicenseVerifier( IServiceProvider serviceProvider )
    {
        var licenseConsumptionManager = (ILicenseConsumptionManager?) serviceProvider.GetService( typeof(ILicenseConsumptionManager));

        if ( licenseConsumptionManager != null )
        {
            this._isLimitedLicense = !licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.Metalama );
        }
        else
        {
            this._isLimitedLicense = false;
        }
    }

    public void VerifyCanAddChildAspect( AspectPredecessor predecessor )
    {
        if ( this._isLimitedLicense )
        {
            switch ( predecessor.Instance )
            {
                case IAspectInstance aspectInstance:
                    var aspectClass = (IAspectClassImpl) aspectInstance.AspectClass;

                    if ( !aspectClass.IsFreemium && !HasRedistributionLicense( aspectInstance.Aspect.GetType().Assembly ) )
                    {
                        throw new InvalidOperationException(
                            $"The '{aspectInstance.AspectClass.ShortName}' aspect cannot add a child aspect because you are using the Metalama Essentials license and the aspect is not marked as [Freemium]." );
                    }

                    break;

                case IFabricInstance fabricInstance:
                    throw new InvalidOperationException(
                        $"The '{fabricInstance.Fabric.GetType().Name}' fabric cannot add an aspect because this feature is not covered by Metalama Essentials license." );
            }
        }
    }

    private static bool HasRedistributionLicense( Assembly assembly )
    {
        // TODO #30275: Licensing: redistribution license
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
        var freemiumAspects = aspectInstanceResults.Select( a => a.AspectInstance.AspectClass ).Where( c => ((IAspectClassImpl) c).IsFreemium ).ToList();
        var freemiumAspectsCount = freemiumAspects.Count;

        if ( freemiumAspectsCount > _maxFreemiumAspects )
        {
            var freemiumAspectNames = string.Join( ",", freemiumAspects.Select( x => "'" + x.ShortName + "'" ) );

            diagnostics.Report(
                LicensingDiagnosticDescriptors.TooManyFreemiumAspects.CreateRoslynDiagnostic(
                    null,
                    (freemiumAspectsCount, _maxFreemiumAspects, freemiumAspectNames) ) );
        }
    }

    public void VerifyCanBeInherited( AspectClass aspectClass, IAspect? prototype, IDiagnosticAdder diagnosticAdder )
    {
        if ( prototype == null )
        {
            // This happens only with abstract classes.
            return;
        }

        if ( this._isLimitedLicense )
        {
            if ( aspectClass.IsInherited && !aspectClass.IsFreemium && !HasRedistributionLicense( prototype.GetType().Assembly ) )
            {
                // TODO: report an error.
            }
        }
    }
}