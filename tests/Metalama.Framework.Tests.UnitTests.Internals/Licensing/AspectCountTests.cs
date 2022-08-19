// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class AspectCountTests : LicensingTestsBase
    {
        [Theory]
        [InlineData(TestLicenseKeys.MetalamaUltimateEssentials, 3, true)]
        [InlineData( TestLicenseKeys.MetalamaUltimateEssentials, 4, false )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 5, true )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 6, false )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 10, true )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 11, false )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, 11, true )]
        public async Task CompilationPassesWithNumberOfAspectsAsync(string licenseKey, int numberOfAspects, bool shouldPass )
        {
            const string usingsAndOrdering = @"
using Metalama.Framework.Aspects;
using System;

[assembly: AspectOrder( {0} )]
";

            const string aspectPrototype = @"
public class Aspect{0} : OverrideMethodAspect
{{
    public override dynamic? OverrideMethod()
    {{
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect{0}));
        return meta.Proceed();
    }}
}}
";

            const string targetPrototype = @"
class TargetClass
{{
    {0}
    void TargetMethod()
    {{
    }}
}}
";

            var sourceCodeBuilder = new StringBuilder();
            var customAttributeApplicationBuilder = new StringBuilder();
            var aspectOrderApplicationBuilder = new StringBuilder();

            for ( var i = 1; i <= numberOfAspects; i++ )
            {
                aspectOrderApplicationBuilder.Append( $"typeof(Aspect{i}) " );

                if ( i < numberOfAspects )
                {
                    aspectOrderApplicationBuilder.Append( ", " );
                }
            }

            sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, usingsAndOrdering, aspectOrderApplicationBuilder.ToString() ) );

            for ( var i = 1; i <= numberOfAspects; i++ )
            {
                sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, aspectPrototype, i ) );
#pragma warning disable SA1114 // Parameter list should follow declaration
                customAttributeApplicationBuilder.AppendLine(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"[Aspect{i}]" );
#pragma warning restore SA1114 // Parameter list should follow declaration
            }

            sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, targetPrototype, customAttributeApplicationBuilder.ToString() ) );

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
    }
}