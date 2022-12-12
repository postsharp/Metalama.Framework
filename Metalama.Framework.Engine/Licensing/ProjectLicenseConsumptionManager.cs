// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Wraps <see cref="IProjectLicenseConsumptionManager"/> into a project-scoped service.
/// </summary>
internal sealed class ProjectLicenseConsumptionManager : IProjectLicenseConsumptionManager
{
    private readonly ILicenseConsumptionManager _impl;

    public ProjectLicenseConsumptionManager( LicensingInitializationOptions options )
    {
        this._impl = BackstageServiceFactory.CreateLicenseConsumptionManager( options );
    }

    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => this._impl.CanConsume( requirement, consumerNamespace );

    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
        => this._impl.ValidateRedistributionLicenseKey( redistributionLicenseKey, aspectClassNamespace );

    public string? RedistributionLicenseKey => this._impl.RedistributionLicenseKey;

    public IReadOnlyList<LicensingMessage> Messages => this._impl.Messages;
}