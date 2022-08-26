// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
internal class LicenseVerifier : IService
{
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

    public void VerifyCompilationResult( Compilation compilation, ImmutableArray<AspectInstanceResult> aspectInstanceResults, UserDiagnosticSink diagnostics )
    {
        // This is to make the test output deterministic.
        static string NormalizeAssemblyName( string assemblyName )
        {
            var match = Regex.Match( assemblyName, "^(test|dependency)_[0-9a-f]{1,16}$" );
            return match.Success
                ? $"{match.Groups[1]}_XXXXXXXXXXXXXXXX"
                : assemblyName;
        }

        var redistributionAspectClasses = new HashSet<AspectClass>();
        var redistributionAspectClassesPerProject = new Dictionary<CompileTimeProject, HashSet<AspectClass>>();
        var nonredistributionAspectClasses = new HashSet<IAspectClass>();

        foreach ( var aspectInstanceResult in aspectInstanceResults )
        {
            if ( aspectInstanceResult.AspectInstance.AspectClass is AspectClass aspectClass
                && aspectClass.Project != null
                && !string.IsNullOrEmpty( aspectClass.Project.ProjectLicenseInfo.RedistributionLicenseKey ) )
            {
                var projectRedistributionLicenseKey = aspectClass.Project.ProjectLicenseInfo.RedistributionLicenseKey!;

                if ( !this._licenseConsumptionManager.ValidateRedistributionLicenseKey( projectRedistributionLicenseKey, aspectClass.FullName ) )
                {
                    diagnostics.Report(
                        LicensingDiagnosticDescriptors.RedistributionLicenseInvalid.CreateRoslynDiagnostic(
                            null, (NormalizeAssemblyName( aspectClass.Project.RunTimeIdentity.Name ), aspectClass.ShortName) ) );
                }
                else
                {
                    if ( !redistributionAspectClassesPerProject.TryGetValue( aspectClass.Project, out var aspects ) )
                    {
                        aspects = new();
                        redistributionAspectClassesPerProject.Add( aspectClass.Project, aspects );
                    }

                    redistributionAspectClasses.Add( aspectClass );
                    aspects.Add( aspectClass );

                    continue;
                }
            }
            
            nonredistributionAspectClasses.Add( aspectInstanceResult.AspectInstance.AspectClass );
        }

        var aspectClassesCount = redistributionAspectClassesPerProject.Count + nonredistributionAspectClasses.Count;
        var namespaceUnlimitedMaxAspectsCount = this._licenseConsumptionManager.GetNamespaceUnlimitedMaxAspectsCount();

        if ( aspectClassesCount <= namespaceUnlimitedMaxAspectsCount )
        {
            return;
        }

        var maxAspectsCountPerNamespace = new Dictionary<string, int>();
        var nonredistributionAspectClassesWithoutNamspaceLimitedLicense = new HashSet<IAspectClass>();
        var nonredistributionAspectClassesPerLicensedNamespace = new Dictionary<string, HashSet<IAspectClass>>();
        var redistributionProjectsPerLicensedNamespace = new Dictionary<string, HashSet<CompileTimeProject>>();
        var redistributionProjectsWithoutNamespaceLimitedLicense = new HashSet<CompileTimeProject>();

        foreach ( var aspectInstanceResult in aspectInstanceResults )
        {
            var aspectClass = aspectInstanceResult.AspectInstance.AspectClass;
            var targetSymbol = aspectInstanceResult.AspectInstance.TargetDeclaration.GetSymbol( compilation );
            var consumerNamespaceSymbol = targetSymbol is INamespaceSymbol ? (INamespaceSymbol)targetSymbol : targetSymbol?.ContainingNamespace;
            var consumerNamespace = consumerNamespaceSymbol?.ToString();

            if ( string.IsNullOrEmpty( consumerNamespace )
                || !this._licenseConsumptionManager.TryGetNamespaceLimitedMaxAspectsCount(
                    consumerNamespace!, out var maxAspectsCount, out var licensedNamespace ) )
            {
                if ( redistributionAspectClasses.Contains( aspectClass ) )
                {
                    redistributionProjectsWithoutNamespaceLimitedLicense.Add( ((AspectClass) aspectClass).Project! );
                }
                else
                {
                    nonredistributionAspectClassesWithoutNamspaceLimitedLicense.Add( aspectClass );
                }
                continue;
            }

            maxAspectsCountPerNamespace[licensedNamespace] = maxAspectsCount;

            if ( redistributionAspectClasses.Contains( aspectClass ) )
            {
                if ( !redistributionProjectsPerLicensedNamespace.TryGetValue( licensedNamespace, out var projects ) )
                {
                    projects = new();
                    redistributionProjectsPerLicensedNamespace.Add( licensedNamespace, projects );
                }

                projects.Add( ((AspectClass) aspectClass).Project! );
            }
            else
            {
                if ( !nonredistributionAspectClassesPerLicensedNamespace.TryGetValue( licensedNamespace, out var aspectClasses ) )
                {
                    aspectClasses = new();
                    nonredistributionAspectClassesPerLicensedNamespace.Add( licensedNamespace, aspectClasses );
                }

                aspectClasses.Add( aspectClass );
            }
        }

        var anyLicensedNamespaceOverMaxAspectsCount = false;

        foreach ( var licensedNamespaceLimit in maxAspectsCountPerNamespace )
        {
            var nonredistributionAspectClassesCount =
                nonredistributionAspectClassesPerLicensedNamespace.TryGetValue( licensedNamespaceLimit.Key, out var aspectClasses )
                ? aspectClasses.Count : 0;

            var redistributionProjectsCount =
                redistributionProjectsPerLicensedNamespace.TryGetValue( licensedNamespaceLimit.Key, out var projects )
                ? projects.Count : 0;

            var namespaceLimitedAspectsCount = nonredistributionAspectClassesCount + redistributionProjectsCount;

            if ( namespaceLimitedAspectsCount > licensedNamespaceLimit.Value )
            {
                anyLicensedNamespaceOverMaxAspectsCount = true;
                break;
            }
        }

        var aspectClassesWithouNamespaceLimitedLicenseCount =
            nonredistributionAspectClassesWithoutNamspaceLimitedLicense.Count
            + redistributionProjectsWithoutNamespaceLimitedLicense.Count;

        if ( aspectClassesWithouNamespaceLimitedLicenseCount <= namespaceUnlimitedMaxAspectsCount && !anyLicensedNamespaceOverMaxAspectsCount )
        {
            return;
        }

        var maxAspectsCountDescriptions = new List<string>();

        if ( namespaceUnlimitedMaxAspectsCount > 0 )
        {
            maxAspectsCountDescriptions.Add( namespaceUnlimitedMaxAspectsCount.ToString( CultureInfo.InvariantCulture ) );
        }

        foreach ( var licensedNamespaceLimit in this._licenseConsumptionManager.GetNamespaceLimitedMaxAspectCounts() )
        {
            maxAspectsCountDescriptions.Add( $"{(licensedNamespaceLimit.MaxAspectsCount < int.MaxValue ? licensedNamespaceLimit.MaxAspectsCount : "aspects used")} in '{licensedNamespaceLimit.LicensedNamespace}' namespace" );
        }

        var maxAspectsCountDescription = string.Join( " and ", maxAspectsCountDescriptions );

        static string GetNames( IEnumerable<IAspectClass> aspectClasses )
        {
            var aspectClassesList = aspectClasses.Select( a => $"'{a.ShortName}'" ).ToList();
            aspectClassesList.Sort();
            return string.Join( ", ", aspectClassesList );
        }

        var aspectClassNames = string.Join( ", ", GetNames( nonredistributionAspectClasses ) );

        if ( redistributionAspectClassesPerProject.Count > 0 )
        {
            if ( aspectClassNames.Length > 0 )
            {
                aspectClassNames += ", ";
            }

            aspectClassNames += string.Join( ", ", redistributionAspectClassesPerProject.Select(
                r => $"aspects from '{NormalizeAssemblyName( r.Key.RunTimeIdentity.Name )}' assembly counted as one ({GetNames( r.Value )})" ) );
        }

        diagnostics.Report(
            LicensingDiagnosticDescriptors.TooManyAspectClasses.CreateRoslynDiagnostic(
                null,
                (aspectClassesCount, maxAspectsCountDescription, aspectClassNames) ) );
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

    public void VerifyCanUseSdk( IAspectWeaver aspectWeaver, IEnumerable<IAspectInstance> aspectInstances, IDiagnosticAdder diagnostics )
    {
        if ( !this._licenseConsumptionManager.CanConsumeFeatures( LicensedFeatures.MetalamaSdk ) )
        {
            var aspectClasses = string.Join( ", ", aspectInstances.Select( i => $"'{i.AspectClass.ShortName}'" ) );

            diagnostics.Report(
                LicensingDiagnosticDescriptors.SdkNotAvailable.CreateRoslynDiagnostic(
                    null, (aspectWeaver.GetType().Name, aspectClasses) ) );
        }
    }
}