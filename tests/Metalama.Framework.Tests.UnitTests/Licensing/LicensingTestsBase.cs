// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public abstract class LicensingTestsBase : UnitTestClass
    {
        private readonly ITestOutputHelper _logger;

        protected LicensingTestsBase( ITestOutputHelper logger ) : base( logger )
        {
            this._logger = logger;
        }

        protected async Task<(DiagnosticBag, List<ToastNotification>)> GetDiagnosticsAndNotificationsAsync(
            string code,
            string? licenseKey,
            string? assemblyName = "AspectCountTests.ArbitraryNamespace",
            string projectName = "TestProject" )
        {
            var mocks = new AdditionalServiceCollection();
            mocks.ProjectServices.Add( sp => sp.AddProjectLicenseConsumptionManagerForTest( licenseKey ) );
            
            mocks.BackstageServices.Add<ToastNotificationsTestServices>(
                p =>
                {
                    var backstageUserInterfaceTestServices = new ToastNotificationsTestServices( this._logger, p, licenseKey );

                    return backstageUserInterfaceTestServices;
                } );
            
            mocks.BackstageServices.Add<IToastNotificationDetectionService>(
                p =>
                {
                    var toastNotificationsTestServices = p.GetRequiredService<ToastNotificationsTestServices>();

                    var toastNotificationDetectionService =
                        toastNotificationsTestServices.Provider.GetRequiredBackstageService<IToastNotificationDetectionService>();

                    return toastNotificationDetectionService;
                } );
            
            var testContextOptions = this.GetDefaultTestContextOptions() with { ProjectName = projectName };

            using var testContext = this.CreateTestContext( testContextOptions, mocks );
            var domain = testContext.Domain;

            var inputCompilation = TestCompilationFactory.CreateCSharpCompilation( code, name: assemblyName );

            using var compileTimePipeline = new CompileTimeAspectPipeline(
                testContext.ServiceProvider,
                domain,
                ExecutionScenario.CompileTime );

            var diagnostics = new DiagnosticBag();
            _ = await compileTimePipeline.ExecuteAsync( diagnostics, inputCompilation, default );

            if ( diagnostics.Count == 0 )
            {
                this.TestOutput.WriteLine( "No diagnostics reported." );
            }
            else
            {
                foreach ( var d in diagnostics )
                {
                    this.TestOutput.WriteLine( $"{d.WarningLevel} {d.Id} {d.GetMessage( CultureInfo.InvariantCulture )}" );
                }
            }

            var toastNotificationsTestServices = testContext.ServiceProvider.Global.GetRequiredBackstageService<ToastNotificationsTestServices>();
            var notifications = toastNotificationsTestServices.Notifications;

            return (diagnostics, notifications);
        }

        protected static string? GetLicenseKey( string? name )
        {
            if ( name == null )
            {
                return null;
            }

            return TestLicenseKeys.GetLicenseKey( name );
        }
    }
}