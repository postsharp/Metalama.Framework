// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Engine.Services;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Wraps <see cref="IProjectLicenseConsumptionService"/> into a project-scoped service.
/// </summary>
internal sealed class ProjectLicenseConsumptionService : IProjectLicenseConsumptionService
{
    private readonly ILicenseConsumptionService _impl;

    // This constructor is used in all scenarios but compile time.
    public ProjectLicenseConsumptionService( ILicenseConsumptionService impl )
    {
        this._impl = impl;
        this._impl.Changed += this.OnChanged;
    }

    private void OnChanged()
    {
        this.Changed?.Invoke();
    }

    // This constructor is used in the compile-time scenario.
    public ProjectLicenseConsumptionService( in ProjectServiceProvider serviceProvider )
    {
        this._impl = serviceProvider.Global.GetRequiredBackstageService<ILicenseConsumptionService>();
    }

    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => this._impl.CanConsume( requirement, consumerNamespace );

    public bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace )
        => this._impl.ValidateRedistributionLicenseKey( redistributionLicenseKey, aspectClassNamespace );

    ILicenseConsumptionService ILicenseConsumptionService.WithAdditionalLicense( string? licenseKey ) => throw new NotSupportedException();

    ILicenseConsumptionService ILicenseConsumptionService.WithoutLicense() => throw new NotSupportedException();

    public IReadOnlyList<LicensingMessage> Messages => this._impl.Messages;

    public bool IsTrialLicense => this._impl.IsTrialLicense;

    public bool IsRedistributionLicense => this._impl.IsRedistributionLicense;

    public string? LicenseString => this._impl.LicenseString;

    public event Action? Changed;
}