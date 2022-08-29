// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class CompilationLicensingTests : LicensingTestsBase
    {
        public CompilationLicensingTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [InlineData( TestLicenseKeys.PostSharpEssentials )]
        [InlineData( TestLicenseKeys.PostSharpFramework )]
        [InlineData( TestLicenseKeys.PostSharpUltimate )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness )]
        public async Task CompilationPassesWithValidLicenseAsync( string licenseKey )
        {
            const string code = @"
using System;

namespace HelloWorld;

class Test
{
    void SayHello()
    {
        Console.WriteLine(""Hello world!"");
    }
}";

            var diagnostics = await this.GetDiagnosticsAsync( code, licenseKey );

            Assert.Empty( diagnostics );
        }
    }
}