// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Licensing;

/// <summary>
/// Wraps <see cref="IProjectLicenseConsumer"/> into a project-scoped service.
/// </summary>
internal sealed class ProjectLicenseConsumer : IProjectLicenseConsumer
{
    private readonly string? _projectLicenseKey;
    private readonly LicenseSourceKind _ignoredLicenseKinds;
    private ILicenseConsumer _consumer;

    public static ProjectLicenseConsumer Create(
        ILicenseConsumptionService licenseConsumptionService,
        string? projectLicenseKey = null,
        LicenseSourceKind ignoredLicenseKinds = LicenseSourceKind.None,
        Action<Diagnostic>? reportDiagnostic = null )
    {
        var consumer = licenseConsumptionService.CreateConsumer( projectLicenseKey, ignoredLicenseKinds, out var messages );

        messages.Report( reportDiagnostic );

        return new ProjectLicenseConsumer( licenseConsumptionService, consumer, projectLicenseKey, ignoredLicenseKinds );
    }

    private ProjectLicenseConsumer(
        ILicenseConsumptionService service,
        ILicenseConsumer consumer,
        string? projectLicenseKey,
        LicenseSourceKind ignoredLicenseKinds )
    {
        this.Service = service;
        this._consumer = consumer;
        this._projectLicenseKey = projectLicenseKey;
        this._ignoredLicenseKinds = ignoredLicenseKinds;
        this.Service.Changed += this.OnChanged;
    }

    private void OnChanged()
    {
        // When we auto-refresh (at design time), we don't propagate error messages.
        this._consumer = this.Service.CreateConsumer( this._projectLicenseKey, this._ignoredLicenseKinds );
        this.Changed?.Invoke();
    }

    public bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null ) => this._consumer.CanConsume( requirement, consumerNamespace );

    public bool IsTrialLicense => this._consumer.IsTrialLicense;

    public bool IsRedistributionLicense => this._consumer.IsRedistributionLicense;

    public string? LicenseString => this._consumer.LicenseString;

    // ReSharper disable once EventNeverSubscribedTo.Global
    public event Action? Changed;

    public ILicenseConsumptionService Service { get; }
}