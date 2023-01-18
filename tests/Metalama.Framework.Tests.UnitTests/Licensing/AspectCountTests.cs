﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class AspectCountTests : LicensingTestsBase
    {
        private const string _arbitraryNamespace = "AspectCountTests.ArbitraryNamespace";
        private const string _tooManyAspectsErrorId = "LAMA0800";
        private const string _redistributionInvalidErrorId = "LAMA0803";

        public AspectCountTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [InlineData( TestLicenseKeys.PostSharpEssentials, 1, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectsErrorId )]
        [InlineData( TestLicenseKeys.PostSharpFramework, 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.PostSharpFramework, 11, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectsErrorId )]
        [InlineData( TestLicenseKeys.PostSharpUltimate, 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal, 3, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal, 4, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectsErrorId )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 5, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, 6, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectsErrorId )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, 11, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectsErrorId )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution, 1, _arbitraryNamespace, _arbitraryNamespace, _redistributionInvalidErrorId )]
        [InlineData(
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution,
            1,
            _arbitraryNamespace,
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistributionNamespace,
            _redistributionInvalidErrorId )]
        [InlineData(
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution,
            11,
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistributionNamespace,
            _arbitraryNamespace,
            null )]
        [InlineData(
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution,
            11,
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistributionNamespace,
            TestLicenseKeys.MetalamaUltimateOpenSourceRedistributionNamespace,
            null )]
        public async Task CompilationPassesWithNumberOfAspectsAsync(
            string licenseKey,
            int numberOfAspects,
            string aspectNamespace,
            string targetNamespace,
            string? expectedErrorId )
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

            sourceCodeBuilder.AppendLine(
                string.Format( CultureInfo.InvariantCulture, targetPrototype, targetNamespace, customAttributeApplicationBuilder.ToString() ) );

            var diagnostics = await this.GetDiagnosticsAsync( sourceCodeBuilder.ToString(), licenseKey, aspectNamespace );

            if ( expectedErrorId == null )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == expectedErrorId );
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

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

            Assert.Empty( diagnostics );
        }
    }
}