// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.UserInterface;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Controls that the project respects the license and reports diagnostics if not.
/// </summary>
public sealed class LicenseVerifier : IProjectService
{
    private const string _licenseUsageSubdirectoryName = "LicenseUsage";
    internal const string LicenseUsageFilePrefix = "usage-";

    private readonly IProjectLicenseConsumptionService _licenseConsumptionService;
    private readonly ITempFileManager _tempFileManager;
    private readonly IToastNotificationDetectionService? _toastNotificationDetectionService;
    private readonly IProjectOptions _projectOptions;

    private readonly Dictionary<CompileTimeProject, RedistributionLicenseFeatures> _redistributionLicenseFeaturesByProject = new();
    private int _reportedErrorCount;

    private readonly struct RedistributionLicenseFeatures;

    private static string GetConsumptionDataDirectory( ITempFileManager tempFileManager )
        => tempFileManager.GetTempDirectory(
            _licenseUsageSubdirectoryName,
            CleanUpStrategy.FileOneMonthAfterCreation,
            versionScope: TempFileVersionScope.None );

    [PublicAPI]
    public static IEnumerable<string> GetConsumptionDataFiles( ITempFileManager tempFileManager )
        => Directory.GetFiles( GetConsumptionDataDirectory( tempFileManager ), $"{LicenseUsageFilePrefix}*.json" );

    internal LicenseVerifier( in ProjectServiceProvider serviceProvider )
    {
        this._licenseConsumptionService = serviceProvider.GetRequiredService<IProjectLicenseConsumptionService>();
        this._tempFileManager = serviceProvider.Global.GetRequiredBackstageService<ITempFileManager>();
        this._toastNotificationDetectionService = serviceProvider.Global.GetBackstageService<IToastNotificationDetectionService>();
        this._projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();
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

    private bool IsValidRedistributionProject( CompileTimeProject project, IDiagnosticAdder diagnosticAdder, IProjectLicenseConsumptionService service )
    {
        var projectAssemblyName = NormalizeAssemblyName( project.RunTimeIdentity.Name );

        var licenseKey = project.ProjectLicenseInfo.RedistributionLicenseKey;

        if ( string.IsNullOrEmpty( licenseKey ) )
        {
            return false;
        }

        if ( !service.ValidateRedistributionLicenseKey( licenseKey, projectAssemblyName ) )
        {
            Interlocked.Increment( ref this._reportedErrorCount );
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
            if ( this.IsValidRedistributionProject( closureProject, diagnosticAdder, this._licenseConsumptionService ) )
            {
                this._redistributionLicenseFeaturesByProject.Add( closureProject, default );
            }
        }

        return true;
    }

    private bool IsProjectWithValidRedistributionLicense( CompileTimeProject project ) => this._redistributionLicenseFeaturesByProject.ContainsKey( project );

    private bool CanConsumeForCurrentProject( LicenseRequirement requirement )
        => this._licenseConsumptionService.CanConsume( requirement, this._projectOptions.ProjectName );

    internal void VerifyCanAddChildAspect( in AspectPredecessor predecessor ) => this.VerifyFabric( predecessor, "add an aspect" );

    internal void VerifyCanAddValidator( in AspectPredecessor predecessor ) => this.VerifyFabric( predecessor, "add a validator" );

    private void VerifyFabric( in AspectPredecessor predecessor, string feature )
    {
        if ( !this.CanConsumeForCurrentProject( LicenseRequirement.Starter ) )
        {
            if ( predecessor.Instance is FabricInstance fabricInstance
                 && !(fabricInstance.Driver is ProjectFabricDriver { Kind: FabricKind.Transitive } fabricDriver
                      && this.IsProjectWithValidRedistributionLicense( fabricDriver.CompileTimeProject )) )
            {
                Interlocked.Increment( ref this._reportedErrorCount );

                throw new DiagnosticException(
                    LicensingDiagnosticDescriptors.FabricsNotAvailable.CreateRoslynDiagnostic(
                        null,
                        (fabricInstance.Fabric.GetType().Name, feature) ) );
            }
        }
    }

    public bool VerifyCanApplyCodeFix( IAspectClass aspectClass )
        => aspectClass switch
        {
            IAspectClassImpl { Project: not null } aspectClassImpl when this.IsProjectWithValidRedistributionLicense( aspectClassImpl.Project )
                => true,

            _ => this.CanConsumeForCurrentProject( LicenseRequirement.Professional )
        };

    internal bool VerifyCanApplyLiveTemplate( in ProjectServiceProvider serviceProvider, IAspectClass aspectClass, IDiagnosticAdder diagnostics )
    {
        var manager = serviceProvider.GetService<IProjectLicenseConsumptionService>();

        if ( manager == null )
        {
            return true;
        }

        return aspectClass switch
        {
            IAspectClassImpl { Project: not null } aspectClassImpl when this.IsValidRedistributionProject( aspectClassImpl.Project, diagnostics, manager )
                => true,

            _ => manager.CanConsume( LicenseRequirement.Professional, serviceProvider.GetService<IProjectOptions>()?.ProjectName )
        };
    }

    internal void VerifyCompilationResult(
        CompileTimeProject project,
        IEnumerable<AspectInstanceResult> aspectInstanceResults,
        bool compilationHasValidator,
        UserDiagnosticSink diagnostics )
    {
        var aspectInstanceResultsArray = aspectInstanceResults.ToImmutableArray();

        // List all aspect classed, that are used.
        var aspectClasses = aspectInstanceResultsArray

            // Don't count skipped instances.
            .Where(
                r => !r.AspectInstance.IsSkipped

                     // Don't count child aspects
                     && (r.AspectInstance.PredecessorDegree == 0

                         // that are not public
                         || r.AspectInstance.AspectClass.Type.IsPublic

                         // and don't inherit from Attribute class
                         || typeof(Attribute).IsAssignableFrom( r.AspectInstance.AspectClass.Type )

                         // and are not applied in other way than as a child aspect.
                         || r.AspectInstance.Predecessors.Any( p => p.Kind != AspectPredecessorKind.ChildAspect )) )
            .Select( r => r.AspectInstance.AspectClass )
            .ToHashSet();

        // Let all contracts to be used for free.
        aspectClasses.RemoveWhere( c => typeof(ContractAspect).IsAssignableFrom( c.Type ) );

        // All aspects from redistributable libraries are for free.
        aspectClasses.RemoveWhere( c => c is AspectClass { Project: { } aspectProject } && this.IsProjectWithValidRedistributionLicense( aspectProject ) );

        // List remaining aspect classes.
        var consumedAspectClassNames =
            aspectClasses
                .SelectAsArray( x => x.FullName )
                .OrderBy( x => x )
                .ToReadOnlyList();

        var hasLicenseError = false;

        // Check the use of Metalama.Framework.Sdk.
        if ( consumedAspectClassNames.Count > 0 )
        {
            if ( project.ClosureReferencesMetalamaSdk )
            {
                if ( !this.CanConsumeForCurrentProject( LicenseRequirement.Professional ) )
                {
                    Interlocked.Increment( ref this._reportedErrorCount );
                    diagnostics.Report( LicensingDiagnosticDescriptors.RoslynApiNotAvailable.CreateRoslynDiagnostic( null, default ) );
                }

                hasLicenseError = true;
            }
        }

        // Check availability of a license and enforce maximal number of classes.
        var maxAspectClasses = this switch
        {
            _ when this.CanConsumeForCurrentProject( LicenseRequirement.Ultimate ) => int.MaxValue,
            _ when this.CanConsumeForCurrentProject( LicenseRequirement.Professional ) => 10,
            _ when this.CanConsumeForCurrentProject( LicenseRequirement.Starter ) => 5,
            _ when this.CanConsumeForCurrentProject( LicenseRequirement.Free ) => 3,
            _ => 0
        };

        // We need to check for reported license errors here,
        // because aspect or validator instances might not be created when not allowed by the registered license.
        var areLicensedFeaturesUsed = aspectInstanceResultsArray.Length > 0 || compilationHasValidator || this._reportedErrorCount > 0 || hasLicenseError;

        if ( maxAspectClasses == 0 )
        {
            // We don't fail on missing license when no licensed features are used. 
            if ( areLicensedFeaturesUsed )
            {
                hasLicenseError = true;

                // Possible causes can be:
                // 1. No license string has been registered.
                // 2. We have a license string, but one not eligible for Metalama.
                // 3. We have a project-bound license string and the project name does not match.

                var diagnostic = string.IsNullOrEmpty( this._licenseConsumptionService.LicenseString )
                    ? LicensingDiagnosticDescriptors.NoLicenseKeyRegistered.CreateRoslynDiagnostic( null, null )
                    : LicensingDiagnosticDescriptors.InvalidLicenseKeyRegistered.CreateRoslynDiagnostic(
                        null,
                        this._licenseConsumptionService.LicenseString! );

                diagnostics.Report( diagnostic );
            }
        }
        else
        {
            if ( consumedAspectClassNames.Count > maxAspectClasses )
            {
                hasLicenseError = true;

                if ( string.IsNullOrEmpty( this._licenseConsumptionService.LicenseString ) )
                {
                    diagnostics.Report( LicensingDiagnosticDescriptors.NoLicenseKeyRegistered.CreateRoslynDiagnostic( null, null ) );
                }
                else
                {
                    diagnostics.Report(
                        LicensingDiagnosticDescriptors.TooManyAspectClasses.CreateRoslynDiagnostic(
                            null,
                            (consumedAspectClassNames.Count, maxAspectClasses, this._projectOptions.ProjectName ?? "Anonymous") ) );
                }
            }
        }

        // Show toast notifications if needed and if Metalama is used in the project.
        if ( areLicensedFeaturesUsed )
        {
            this.DetectToastNotifications();
        }

        // Write consumption data to disk if required.
        if ( hasLicenseError && (this._projectOptions.WriteLicenseUsageData ?? this._licenseConsumptionService.IsTrialLicense) )
        {
            var directory = GetConsumptionDataDirectory( this._tempFileManager );

            var file = new LicenseConsumptionFile(
                this._projectOptions.ProjectPath ?? "Anonymous",
                this._projectOptions.Configuration ?? "",
                this._projectOptions.TargetFramework ?? "",
                consumedAspectClassNames.Count,
                consumedAspectClassNames,
                EngineAssemblyMetadataReader.Instance.PackageVersion ?? "",
                EngineAssemblyMetadataReader.Instance.BuildDate );

            file.WriteToDirectory( directory );
        }
    }

    internal void DetectToastNotifications()
        => this._toastNotificationDetectionService?.Detect(
            new ToastNotificationDetectionOptions { HasValidLicense = !string.IsNullOrEmpty( this._licenseConsumptionService.LicenseString ) } );
}