// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Framework.Engine.Licensing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class ValidatorsLicensingTests : LicensingTestsBase
    {
        private const string _noLicenseKeyErrorId = LicensingDiagnosticDescriptors.NoLicenseKeyRegisteredId;
        private const string _fabricsNotAvailableErrorId = LicensingDiagnosticDescriptors.FabricsNotAvailableId;
        
        private const string _declarationValidationAspectAppliedCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace DeclarationValidatorTests;

class ValidateAspect : TypeAspect
{
    private static readonly DiagnosticDefinition<IDeclaration> _error = new(
            ""DEMO01"",
            Severity.Error,
            ""'{0}' is validated."" );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Outbound.SelectMany( t => t.Methods ).AfterAllAspects().Validate( Validate );
    }

    private static void Validate( in DeclarationValidationContext context )
    {
        context.Diagnostics.Report( _error.WithArguments( context.Declaration ) );
    }
}

[ValidateAspect]
class TargetClass
{
    void TargetMethod() { }
}
";

        private const string _declarationValidationFabricAppliedCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;

namespace DeclarationValidatorTests;

class Fabric : ProjectFabric
{
    private static readonly DiagnosticDefinition<IDeclaration> _error = new(
            ""DEMO02"",
            Severity.Error,
            ""'{0}' is validated."" );

    public override void AmendProject( IProjectAmender project )
    {
        project.Outbound.Validate( Validate );
    }

    private static void Validate( in DeclarationValidationContext context )
    {
        context.Diagnostics.Report( _error.WithArguments( context.Declaration ) );
    }
}

class TargetClass
{
    void TargetMethod() { }
}
";

        public ValidatorsLicensingTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [InlineData( null, _noLicenseKeyErrorId )]
        [InlineData( nameof(LicenseKeys.PostSharpFramework), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.PostSharpUltimate), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaFreePersonal), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaStarterBusiness), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaProfessionalBusiness), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimateBusiness), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimateOpenSourceRedistribution), "DEMO01" )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimatePersonalProjectBound), LicensingDiagnosticDescriptors.InvalidLicenseKeyRegisteredId )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimatePersonalProjectBound), "DEMO01", TestLicenseKeyProvider.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaAspectAsync( string? licenseKeyName, string expectedDiagnosticId, string projectName = "TestProject" )
        {
            var licenseKey = GetLicenseKey( licenseKeyName );

            var diagnostics = await this.GetDiagnosticsAsync(
                _declarationValidationAspectAppliedCode,
                licenseKey,
                projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == expectedDiagnosticId );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Theory]
        [InlineData( null, _noLicenseKeyErrorId )]
        [InlineData( nameof(LicenseKeys.PostSharpFramework), "DEMO02" )]
        [InlineData( nameof(LicenseKeys.PostSharpUltimate), "DEMO02" )]
        [InlineData( nameof(LicenseKeys.MetalamaFreePersonal), _fabricsNotAvailableErrorId )]
        [InlineData( nameof(LicenseKeys.MetalamaStarterBusiness), "DEMO02" )]
        [InlineData( nameof(LicenseKeys.MetalamaProfessionalBusiness), "DEMO02" )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimateBusiness), "DEMO02" )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimatePersonalProjectBound), _fabricsNotAvailableErrorId )]
        [InlineData( nameof(LicenseKeys.MetalamaUltimatePersonalProjectBound), "DEMO02", TestLicenseKeyProvider.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaFabricAsync( string licenseKeyName, string expectedDiagnosticId, string projectName = "TestProject" )
        {
            var licenseKey = GetLicenseKey( licenseKeyName );
            
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationFabricAppliedCode, licenseKey, projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == expectedDiagnosticId );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }
        
        [Fact]
        public async Task NotificationsAreTriggeredWhenOnlyValidatorsAreUsedAsync()
        {
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationAspectAppliedCode, LicenseKeys.MetalamaUltimateBusiness );

            Assert.Single( diagnostics, d => d.Id == "DEMO01" );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }
    }
}