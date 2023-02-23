﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
public sealed class LicenseVerifier : IProjectService
{
    private const string _licenseCreditsSubdirectoryName = "LicenseCredits";
    internal const string LicenseCreditsFilePrefix = "credits-";

    private readonly IProjectLicenseConsumptionService _licenseConsumptionService;
    private readonly IProjectOptions _projectOptions;
    private readonly Dictionary<CompileTimeProject, RedistributionLicenseFeatures> _redistributionLicenseFeaturesByProject = new();
    private readonly ITempFileManager _tempFileManager;
    private readonly string? _targetAssemblyName;

    private readonly struct RedistributionLicenseFeatures { }

    private static string GetConsumptionDataDirectory( ITempFileManager tempFileManager )
    {
        return tempFileManager.GetTempDirectory( _licenseCreditsSubdirectoryName, CleanUpStrategy.FileOneMonthAfterCreation, versionNeutral: true );
    }

    [PublicAPI]
    public static IEnumerable<string> GetConsumptionDataFiles( ITempFileManager tempFileManager )
    {
        return Directory.GetFiles( GetConsumptionDataDirectory( tempFileManager ), $"{LicenseCreditsFilePrefix}*.json" );
    }

    internal LicenseVerifier( ProjectServiceProvider serviceProvider, string? targetAssemblyName )
    {
        this._licenseConsumptionService = serviceProvider.GetRequiredService<IProjectLicenseConsumptionService>();
        this._tempFileManager = serviceProvider.Global.GetRequiredBackstageService<ITempFileManager>();
        this._projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();
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

    private static bool IsValidRedistributionProject( CompileTimeProject project, IDiagnosticAdder diagnosticAdder, IProjectLicenseConsumptionService service )
    {
        var projectAssemblyName = NormalizeAssemblyName( project.RunTimeIdentity.Name );

        var licenseKey = project.ProjectLicenseInfo.RedistributionLicenseKey;

        if ( string.IsNullOrEmpty( licenseKey ) )
        {
            return false;
        }

        if ( !service.ValidateRedistributionLicenseKey( licenseKey, projectAssemblyName ) )
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
            if ( IsValidRedistributionProject( closureProject, diagnosticAdder, this._licenseConsumptionService ) )
            {
                this._redistributionLicenseFeaturesByProject.Add( closureProject, default );
            }
        }

        return true;
    }

    private bool IsProjectWithValidRedistributionLicense( CompileTimeProject project ) => this._redistributionLicenseFeaturesByProject.ContainsKey( project );

    private bool CanConsumeForCurrentCompilation( LicenseRequirement requirement )
        => this._licenseConsumptionService.CanConsume( requirement, this._targetAssemblyName );

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

    internal void VerifyCanAddValidator( AspectPredecessor predecessor )
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

    internal static bool VerifyCanApplyLiveTemplate( ProjectServiceProvider serviceProvider, IAspectClass aspectClass, IDiagnosticAdder diagnostics )
    {
        var manager = serviceProvider.GetService<IProjectLicenseConsumptionService>();

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

        // Compute required credits.
        var consumptions = new List<LicenseCreditConsumption>();

        consumptions.AddRange(
            nonRedistributionAspectClasses.SelectAsEnumerable( x => x.FullName )
                .OrderBy( x => x )
                .Select( x => new LicenseCreditConsumption( x, 1, LicenseCreditConsumptionKind.UserClass ) ) );

        consumptions.AddRange(
            projectsWithRedistributionLicense.SelectAsEnumerable( x => x.RunTimeIdentity.Name )
                .OrderBy( x => x )
                .Select( x => new LicenseCreditConsumption( x, 1, LicenseCreditConsumptionKind.Library ) ) );

        var totalRequiredCredits = (int) Math.Ceiling( consumptions.Sum( c => c.ConsumedCredits ) );

        // Write consumption data to disk if required.
        if ( this._projectOptions.WriteLicenseCreditData ?? this._licenseConsumptionService.IsTrialLicense )
        {
            var directory = GetConsumptionDataDirectory( this._tempFileManager );

            var file = new LicenseConsumptionFile(
                this._projectOptions.ProjectPath ?? "Anonymous",
                this._projectOptions.Configuration ?? "",
                this._projectOptions.TargetFramework ?? "",
                totalRequiredCredits,
                consumptions,
                EngineAssemblyMetadataReader.Instance.PackageVersion,
                EngineAssemblyMetadataReader.Instance.BuildDate );

            file.WriteToDirectory( directory );
        }

        // Enforce the license.
        var maxCredits = this switch
        {
            _ when this._licenseConsumptionService.CanConsume( LicenseRequirement.Ultimate, compilation.AssemblyName ) => int.MaxValue,
            _ when this._licenseConsumptionService.CanConsume( LicenseRequirement.Professional, compilation.AssemblyName ) => 10,
            _ when this._licenseConsumptionService.CanConsume( LicenseRequirement.Starter, compilation.AssemblyName ) => 5,
            _ when this._licenseConsumptionService.CanConsume( LicenseRequirement.Free, compilation.AssemblyName ) => 3,
            _ => 0
        };

        if ( totalRequiredCredits > maxCredits )
        {
            diagnostics.Report(
                LicensingDiagnosticDescriptors.InsufficientCredits.CreateRoslynDiagnostic(
                    null,
                    (totalRequiredCredits, maxCredits, Path.GetFileNameWithoutExtension( this._projectOptions.ProjectPath ) ?? "Anonymous") ) );
        }
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
        // ILicenseConsumptionService is hacked: this is a project-scoped service because it is instantiate with the license key in the project file,
        // but its interface is backstage because it is implemented in the backstage assembly.
        var manager = serviceProvider.GetService<IProjectLicenseConsumptionService>();

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