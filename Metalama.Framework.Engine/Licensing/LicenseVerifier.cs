// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
public sealed class LicenseVerifier : IProjectService
{
    private readonly IProjectLicenseConsumptionManager _licenseConsumptionManager;
    private readonly Dictionary<CompileTimeProject, RedistributionLicenseFeatures> _redistributionLicenseFeaturesByProject = new();
    private readonly string? _targetAssemblyName;

    private readonly struct RedistributionLicenseFeatures { }

    internal LicenseVerifier( ProjectLicenseConsumptionManager licenseConsumptionManager, string? targetAssemblyName )
    {
        this._licenseConsumptionManager = licenseConsumptionManager;
        this._targetAssemblyName = targetAssemblyName;
    }

    // This is to make the test output deterministic.
    private static string NormalizeAssemblyName( string assemblyName )
    {
        var match = Regex.Match( assemblyName, "^(test|dependency)_[0-9a-f]{1,16}$" );

        // ReSharper disable once StringLiteralTypo
        return match.Success
            ? $"{match.Groups[1]}_XXXXXXXXXXXXXXXX"
            : assemblyName;
    }

    private static bool IsValidRedistributionProject( CompileTimeProject project, IDiagnosticAdder diagnosticAdder, IProjectLicenseConsumptionManager manager )
    {
        var projectAssemblyName = NormalizeAssemblyName( project.RunTimeIdentity.Name );

        var licenseKey = project.ProjectLicenseInfo.RedistributionLicenseKey;

        if ( string.IsNullOrEmpty( licenseKey ) )
        {
            return false;
        }

        if ( !manager.ValidateRedistributionLicenseKey( licenseKey, projectAssemblyName ) )
        {
            diagnosticAdder.Report( LicensingDiagnosticDescriptors.RedistributionLicenseInvalid.CreateRoslynDiagnostic( null, projectAssemblyName ) );

            return false;
        }

        return true;
    }

    internal bool TryInitialize( CompileTimeProject? project, IDiagnosticAdder diagnosticAdder )
    {
        if ( project == null )
        {
            // The project has no aspect class and no reference with aspects.
            return true;
        }

        foreach ( var closureProject in project.ClosureProjects )
        {
            if ( IsValidRedistributionProject( closureProject, diagnosticAdder, this._licenseConsumptionManager ) )
            {
                this._redistributionLicenseFeaturesByProject.Add( closureProject, default );
            }
        }

        return true;
    }

    private bool IsProjectWithValidRedistributionLicense( CompileTimeProject project ) => this._redistributionLicenseFeaturesByProject.ContainsKey( project );

    private bool CanConsumeForCurrentCompilation( LicenseRequirement requirement )
        => this._licenseConsumptionManager.CanConsume( requirement, this._targetAssemblyName );

    internal void VerifyCanAddChildAspect( AspectPredecessor predecessor )
    {
        if ( !this.CanConsumeForCurrentCompilation( LicenseRequirement.Starter ) )
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

    internal void VerifyCanValidator( AspectPredecessor predecessor )
    {
        if ( !this.CanConsumeForCurrentCompilation( LicenseRequirement.Starter ) )
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

    public bool VerifyCanApplyCodeFix( IAspectClass aspectClass )
        => aspectClass switch
        {
            IAspectClassImpl { Project: { } } aspectClassImpl when this.IsProjectWithValidRedistributionLicense( aspectClassImpl.Project )
                => true,

            _ => this.CanConsumeForCurrentCompilation( LicenseRequirement.Professional )
        };

    public static bool VerifyCanApplyLiveTemplate( ProjectServiceProvider serviceProvider, IAspectClass aspectClass, IDiagnosticAdder diagnostics )
    {
        var manager = serviceProvider.GetService<IProjectLicenseConsumptionManager>();

        if ( manager == null )
        {
            return true;
        }

        return aspectClass switch
        {
            IAspectClassImpl { Project: { } } aspectClassImpl when IsValidRedistributionProject( aspectClassImpl.Project, diagnostics, manager )
                => true,

            _ => manager.CanConsume( LicenseRequirement.Professional )
        };
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
            .Where( this.IsProjectWithValidRedistributionLicense )
            .ToHashSet();

        nonRedistributionAspectClasses.RemoveWhere( c => c is AspectClass { Project: { } } ac && projectsWithRedistributionLicense.Contains( ac.Project ) );

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
                aspectInstanceResults.Select( r => r.AspectInstance.AspectClass )
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

    internal void VerifyCanBeInherited( AspectClass aspectClass, IDiagnosticAdder diagnostics )
    {
        if ( !this.CanConsumeForCurrentCompilation( LicenseRequirement.Starter ) )
        {
            diagnostics.Report( LicensingDiagnosticDescriptors.InheritanceNotAvailable.CreateRoslynDiagnostic( null, aspectClass.ShortName ) );
        }
    }

    internal static void VerifyCanUseSdk(
        ProjectServiceProvider serviceProvider,
        IAspectWeaver aspectWeaver,
        IEnumerable<IAspectInstance> aspectInstances,
        IDiagnosticAdder diagnostics )
    {
        // ILicenseConsumptionManager is hacked: this is a project-scoped service because it is instantiate with the license key in the project file,
        // but its interface is backstage because it is implemented in the backstage assembly.
        var manager = serviceProvider.GetService<IProjectLicenseConsumptionManager>();

        if ( manager == null )
        {
            return;
        }

        if ( !manager.CanConsume( LicenseRequirement.Professional ) )
        {
            var aspectClasses = string.Join( ", ", aspectInstances.Select( i => $"'{i.AspectClass.ShortName}'" ) );

            diagnostics.Report( LicensingDiagnosticDescriptors.SdkNotAvailable.CreateRoslynDiagnostic( null, (aspectWeaver.GetType().Name, aspectClasses) ) );
        }
    }
}