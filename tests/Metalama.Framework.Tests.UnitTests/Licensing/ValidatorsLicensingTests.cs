// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
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
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), true )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), true )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonalProjectBound), false )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonalProjectBound), true, TestLicenses.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaAspectAsync( string licenseKey, bool accepted, string projectName = "TestProject" )
        {
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationAspectAppliedCode, licenseKey, projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == (accepted ? "DEMO01" : "LAMA0800") );
        }

        [Theory]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), false )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), true )]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), false )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonalProjectBound), false )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonalProjectBound), true, TestLicenses.MetalamaUltimateProjectBoundProjectName )]
        public async Task DeclarationValidatorIsAcceptedViaFabricAsync( string licenseKey, bool accepted, string projectName = "TestProject" )
        {
            var diagnostics = await this.GetDiagnosticsAsync( _declarationValidationFabricAppliedCode, licenseKey, projectName: projectName );

            Assert.Single( diagnostics, d => d.Id == (accepted ? "DEMO02" : "LAMA0801") );
        }
    }
}