// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class AspectCountTests : LicensingTestsBase
    {
        private const string UnlicensedNamespace = "AspectCountTests.UnlicensedNamespace";

        public AspectCountTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [InlineData( TestLicenseKeys.MetalamaUltimateEssentials, 3, UnlicensedNamespace, UnlicensedNamespace, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimateEssentials, 4, UnlicensedNamespace, UnlicensedNamespace, false )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 5, UnlicensedNamespace, UnlicensedNamespace, true )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 6, UnlicensedNamespace, UnlicensedNamespace, false )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 10, UnlicensedNamespace, UnlicensedNamespace, true )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 11, UnlicensedNamespace, UnlicensedNamespace, false )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, 11, UnlicensedNamespace, UnlicensedNamespace, true )]
        [InlineData( TestLicenseKeys.NamespaceLimitedMetalamaUltimateBusiness, 1, TestLicenseKeys.NamespaceLimitedMetalamaUltimateBusinessNamespace, UnlicensedNamespace, false )]
        [InlineData( TestLicenseKeys.NamespaceLimitedMetalamaUltimateEssentials, 3, UnlicensedNamespace, TestLicenseKeys.NamespaceLimitedMetalamaUltimateEssentialsNamespace, true )]
        [InlineData( TestLicenseKeys.NamespaceLimitedMetalamaUltimateEssentials, 4, UnlicensedNamespace, TestLicenseKeys.NamespaceLimitedMetalamaUltimateEssentialsNamespace, false )]
        [InlineData( TestLicenseKeys.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, 1, TestLicenseKeys.NamespaceLimitedMetalamaUltimateOpenSourceRedistributionNamespace, UnlicensedNamespace, false )]
        [InlineData( TestLicenseKeys.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, 11, TestLicenseKeys.NamespaceLimitedMetalamaUltimateOpenSourceRedistributionNamespace, TestLicenseKeys.NamespaceLimitedMetalamaUltimateOpenSourceRedistributionNamespace, true )]
        public async Task CompilationPassesWithNumberOfAspectsAsync(string licenseKey, int numberOfAspects, string aspectNamespace, string targetNamespace, bool shouldPass )
        {
            const string usingsAndOrdering = @"
using Metalama.Framework.Aspects;
using System;

[assembly: AspectOrder( {0} )]
";

            const string aspectPrototype = @"

namespace {0}
{{
    public class Aspect{1} : OverrideMethodAspect
    {{
        public override dynamic? OverrideMethod()
        {{
            Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect{1}));
            return meta.Proceed();
        }}
    }}
}}
";

            const string targetPrototype = @"
namespace {0}
{{
    class TargetClass
    {{
        {1}
        void TargetMethod()
        {{
        }}
    }}
}}
";

            var sourceCodeBuilder = new StringBuilder();
            var customAttributeApplicationBuilder = new StringBuilder();
            var aspectOrderApplicationBuilder = new StringBuilder();

            for ( var i = 1; i <= numberOfAspects; i++ )
            {
#pragma warning disable SA1114 // Parameter list should follow declaration
                aspectOrderApplicationBuilder.AppendLine(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"typeof({aspectNamespace}.Aspect{i}) " );
#pragma warning restore SA1114 // Parameter list should follow declaration

                if ( i < numberOfAspects )
                {
                    aspectOrderApplicationBuilder.Append( ", " );
                }
            }

            sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, usingsAndOrdering, aspectOrderApplicationBuilder.ToString() ) );

            for ( var i = 1; i <= numberOfAspects; i++ )
            {
                sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, aspectPrototype, aspectNamespace, i ) );
#pragma warning disable SA1114 // Parameter list should follow declaration
                customAttributeApplicationBuilder.AppendLine(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"[{aspectNamespace}.Aspect{i}]" );
#pragma warning restore SA1114 // Parameter list should follow declaration
            }

            sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, targetPrototype, targetNamespace, customAttributeApplicationBuilder.ToString() ) );

            var diagnostics = await this.GetDiagnosticsAsync( sourceCodeBuilder.ToString(), licenseKey );

            if ( shouldPass )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == "LAMA0800" );
            }
        }

        [Fact]
        public async Task MultipleUsagesOfOneAspectAcceptedAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using System;

public class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect));
        return meta.Proceed();
    }
}

class TargetClass
{
    [Aspect]
    void TargetMethod1()
    {
    }

    [Aspect]
    void TargetMethod2()
    {
    }

    [Aspect]
    void TargetMethod3()
    {
    }

    [Aspect]
    void TargetMethod4()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaUltimateEssentials );

            Assert.Empty( diagnostics );
        }
    }
}