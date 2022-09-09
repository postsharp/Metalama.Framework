﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public class InheritanceLicensingTests : LicensingTestsBase
    {
        private const string _codeWithInheritedAspect = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Doc.InheritedTypeLevel
{
    [InheritedAspect]
    internal class BaseClass
    {
        public void Method1() { }

        public virtual void Method2() { }
    }

    internal class DerivedClass : BaseClass
    {
        public override void Method2()
        {
            base.Method2();
        }

        public void Method3() { }
    }

    internal class DerivedTwiceClass : DerivedClass
    {
        public override void Method2()
        {
            base.Method2();
        }

        public void Method4() { }
    }

    [Inherited]
    internal class InheritedAspectAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach ( var method in builder.Target.Methods )
            {
                builder.Advice.Override( method, nameof(this.MethodTemplate) );
            }
        }

        [Template]
        private dynamic? MethodTemplate()
        {
            Console.WriteLine( ""Hacked!"" );

            return meta.Proceed();
        }
    }
}
";

        public InheritanceLicensingTests( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [InlineData( TestLicenseKeys.PostSharpEssentials, false )]
        [InlineData( TestLicenseKeys.PostSharpFramework, true )]
        [InlineData( TestLicenseKeys.PostSharpUltimate, true )]
        [InlineData( TestLicenseKeys.MetalamaFreePersonal, false )]
        [InlineData( TestLicenseKeys.MetalamaStarterBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaProfessionalBusiness, true )]
        [InlineData( TestLicenseKeys.MetalamaUltimateBusiness, true )]
        public async Task InheritanceIsAcceptedAsync( string licenseKey, bool accepted )
        {
            var diagnostics = await this.GetDiagnosticsAsync( _codeWithInheritedAspect, licenseKey );

            if ( accepted )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == "LAMA0802" );
            }
        }
    }
}