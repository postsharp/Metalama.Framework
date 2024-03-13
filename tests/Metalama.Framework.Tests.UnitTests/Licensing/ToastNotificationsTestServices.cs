// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing;

internal class ToastNotificationsTestServices : TestsBase, IBackstageService
{
    public IServiceProvider Provider => this.ServiceProvider;
    
    public List<ToastNotification> Notifications => this.UserInterface.Notifications;

    public ToastNotificationsTestServices( ITestOutputHelper logger, IServiceProvider serviceProvider, string? licenseKey ) : base(
        logger,
        serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication )
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;
        
        if ( licenseKey != null )
        {
            Assert.True( this.LicenseRegistrationService.TryRegisterLicense( licenseKey, out var errorMessage ) );
            Assert.Null( errorMessage );
        }
    }
}