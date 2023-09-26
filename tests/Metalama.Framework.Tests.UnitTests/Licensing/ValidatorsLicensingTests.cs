// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class ValidatorsLicensingTests : LicensingTestsBase
    {
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
        project.With( x => x ).Validate( Validate );
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
        [InlineData( TestLicenseKeys.PostSharpEssentials, true )]
        [InlineData( TestLicenseKeys.PostSharpFramework, true )]
        [InlineData( TestLicenseKeys.PostSharpUltimate, true )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal, true )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimatePersonalProjectBound, false )]
        [InlineData( TestLicenseKeys.MetalamaUltimatePersonalProjectBound, true, TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaAspectAsync( string licenseKey, bool accepted, string projectName = "TestProject" )
        {
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationAspectAppliedCode, licenseKey, projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == (accepted ? "DEMO01" : "LAMA0800") );
        }

        [Theory]
        [InlineData( TestLicenseKeys.PostSharpEssentials, false )]
        [InlineData( TestLicenseKeys.PostSharpFramework, true )]
        [InlineData( TestLicenseKeys.PostSharpUltimate, true )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal, false )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimatePersonalProjectBound, false )]
        [InlineData( TestLicenseKeys.MetalamaUltimatePersonalProjectBound, true, TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaFabricAsync( string licenseKey, bool accepted, string projectName = "TestProject" )
        {
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationFabricAppliedCode, licenseKey, projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == (accepted ? "DEMO02" : "LAMA0801") );
        }
    }
}