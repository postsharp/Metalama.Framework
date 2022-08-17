// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class CompilationLicensingTests : LicensingTestsBase
    {
        [Theory]
        [InlineData( TestLicenseKeys.MetalamaUltimateEssentials )]
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