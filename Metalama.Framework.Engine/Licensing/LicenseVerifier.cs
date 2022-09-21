// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
public class LicenseVerifier : IService
{
    private readonly ILicenseConsumptionManager _licenseConsumptionManager;
    private readonly Dictionary<CompileTimeProject, RedistributionLicenseFeatures> _redistributionLicenseFeaturesByProject = new();

    private readonly struct RedistributionLicenseFeatures
    {
        
    }
    internal LicenseVerifier( ILicenseConsumptionManager licenseConsumptionManager )
    {
        this._licenseConsumptionManager = licenseConsumptionManager;
    }
    

    internal bool TryInitialize( CompileTimeProject? project, IDiagnosticAdder diagnosticAdder )
    {
        if ( project == null )
        {
            // The project has no aspect class and no reference with aspects.
            return true;
        }
        
        foreach ( var p in project.ClosureProjects )
        {
            var licenseKey = project.ProjectLicenseInfo.RedistributionLicenseKey;

            if ( string.IsNullOrEmpty( licenseKey ) )
            {
                return false;
            }

            var projectAssemblyName = NormalizeAssemblyName( project.RunTimeIdentity.Name );

            if ( !this._licenseConsumptionManager.ValidateRedistributionLicenseKey( licenseKey!, projectAssemblyName ) )
            {
                diagnosticAdder.Report(
                    LicensingDiagnosticDescriptors.RedistributionLicenseInvalid.CreateRoslynDiagnostic( null, NormalizeAssemblyName( projectAssemblyName ) ) );

                return false;
            }
            
            this._redistributionLicenseFeaturesByProject.Add( p, new RedistributionLicenseFeatures() );
        }
        
        // This is to make the test output deterministic.
        static string NormalizeAssemblyName( string assemblyName )
        {
            var match = Regex.Match( assemblyName, "^(test|dependency)_[0-9a-f]{1,16}$" );

            // ReSharper disable once StringLiteralTypo
            return match.Success
                ? $"{match.Groups[1]}_XXXXXXXXXXXXXXXX"
                : assemblyName;
        }

        return true;
    }

    private bool IsProjectWithValidRedistributionLicense( CompileTimeProject project )
    {
        return this._redistributionLicenseFeaturesByProject.ContainsKey( project );
    }

    internal void VerifyCanAddChildAspect( AspectPredecessor predecessor )
    {
        if ( !this._licenseConsumptionManager.CanConsume( LicenseRequirement.Starter ) )
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

    internal  void VerifyCanValidator( AspectPredecessor predecessor )
    {
        if ( !this._licenseConsumptionManager.CanConsume( LicenseRequirement.Starter ) )
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
    
    public bool CanSuggestCodeFix( IAspectClass aspectClass )
    {
        if ( aspectClass is FabricAggregateAspectClass )
        {
            // TODO: depends on the product?
            return true;
        }
        else if ( aspectClass is IAspectClassImpl aspectClassImpl && aspectClassImpl.Project != null && IsProjectWithValidRedistributionLicense( aspectClassImpl.Project ) )
        {
            return true;
        }
        else
        {
            // TODO: depends on the product?
            return true;
        }
    }


    internal void VerifyCompilationResult( Compilation compilation, ImmutableArray<AspectInstanceResult> aspectInstanceResults, UserDiagnosticSink diagnostics )
    {


        

        // Distinguish redistribution and non-redistribution aspect classes.
        var nonRedistributionAspectClasses = aspectInstanceResults.Select( r => r.AspectInstance.AspectClass ).ToHashSet();

        var projectsWithRedistributionLicense = nonRedistributionAspectClasses
            .OfType<AspectClass>()
            .Where( c => c.Project != null )
            .Select( c => c.Project! )
            .Distinct()
            .Where( IsProjectWithValidRedistributionLicense )
            .ToHashSet();

        nonRedistributionAspectClasses.RemoveWhere(
            c => c is AspectClass ac && ac.Project != null && projectsWithRedistributionLicense.Contains( ac.Project ) );

        // One redistribution library counts as one aspect class.
        var aspectClassesCount = projectsWithRedistributionLicense.Count + nonRedistributionAspectClasses.Count;

        if ( aspectClassesCount == 0 )
        {
            // There are no aspect classes applied.
            return;
        }

        var maxAspectsCount = this switch
        {
            _ when this._licenseConsumptionManager.CanConsume( LicenseRequirement.Ultimate, compilation.AssemblyName ) => int.MaxValue,
            _ when this._licenseConsumptionManager.CanConsume( LicenseRequirement.Professional, compilation.AssemblyName ) => 10,
            _ when this._licenseConsumptionManager.CanConsume( LicenseRequirement.Starter, compilation.AssemblyName ) => 5,
            _ when this._licenseConsumptionManager.CanConsume( LicenseRequirement.Free, compilation.AssemblyName ) => 3,
            _ => 0
        };

        if ( aspectClassesCount <= maxAspectsCount )
        {
            // All aspect classes are covered by the available license.
            return;
        }

        // The count of aspect classes is not covered by the available licenses. Report an error.
        static string GetNames( IEnumerable<IAspectClass> aspectClasses )
        {
            var aspectClassesList = aspectClasses.Select( a => $"'{a.ShortName}'" ).ToList();
            aspectClassesList.Sort();

            return string.Join( ", ", aspectClassesList );
        }

        var aspectClassNames = string.Join( ", ", GetNames( nonRedistributionAspectClasses ) );

        if ( projectsWithRedistributionLicense.Count > 0 )
        {
            aspectClassNames += ", " + string.Join(
                ", ",
                aspectInstanceResults
                    .Select( r => r.AspectInstance.AspectClass )
                    .OfType<AspectClass>()
                    .Where( c => c.Project != null && projectsWithRedistributionLicense.Contains( c.Project ) )
                    .GroupBy( c => c.Project! )
                    .Select( pc => $"aspects from '{NormalizeAssemblyName( pc.Key.RunTimeIdentity.Name )}' assembly counted as one ({GetNames( pc )})" ) );
        }

        diagnostics.Report(
            LicensingDiagnosticDescriptors.TooManyAspectClasses.CreateRoslynDiagnostic(
                null,
                (aspectClassesCount, maxAspectsCount, aspectClassNames) ) );
    }

    internal void VerifyCanBeInherited( AspectClass aspectClass, IAspect? prototype, IDiagnosticAdder diagnostics )
    {
        if ( prototype == null )
        {
            // This happens only with abstract classes.
            return;
        }

        if ( aspectClass.IsInherited && !this._licenseConsumptionManager.CanConsume( LicenseRequirement.Starter ) )
        {
            diagnostics.Report( LicensingDiagnosticDescriptors.InheritanceNotAvailable.CreateRoslynDiagnostic( null, aspectClass.ShortName ) );
        }
    }

    internal void VerifyCanUseSdk( IAspectWeaver aspectWeaver, IEnumerable<IAspectInstance> aspectInstances, IDiagnosticAdder diagnostics )
    {
        if ( !this._licenseConsumptionManager.CanConsume( LicenseRequirement.Professional ) )
        {
            var aspectClasses = string.Join( ", ", aspectInstances.Select( i => $"'{i.AspectClass.ShortName}'" ) );

            diagnostics.Report( LicensingDiagnosticDescriptors.SdkNotAvailable.CreateRoslynDiagnostic( null, (aspectWeaver.GetType().Name, aspectClasses) ) );
        }
    }
}