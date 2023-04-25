// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Licensing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Licensing
{
    public sealed class InheritanceLicensingTests : LicensingTestsBase
    {
        private const string _codeWithNonInstantiatedInheritedAspect = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Doc.InheritedTypeLevel
{
    [Inheritable]
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

        private const string _codeWithInstantiatedInheritedAspect = @"
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

    [Inheritable]
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
        [InlineData( "PostSharp Essentials", TestLicenseKeys.PostSharpEssentials, false )]
        [InlineData( "PostSharp Framework", TestLicenseKeys.PostSharpFramework, true )]
        [InlineData( "PostSharp Ultimate", TestLicenseKeys.PostSharpUltimate, true )]
        [InlineData( "Metalama Free Personal", TestLicenseKeys.MetalamaFreePersonal, false )]
        [InlineData( "Metalama Starter Business", TestLicenseKeys.MetalamaStarterBusiness, true )]
        [InlineData( "Metalama Professional Business", TestLicenseKeys.MetalamaProfessionalBusiness, true )]
        [InlineData( "Metalama Ultimate Business", TestLicenseKeys.MetalamaUltimateBusiness, true )]
        public async Task InheritanceIsAcceptedAsync( string licenseName, string licenseKey, bool accepted )
        {
            _ = licenseName;

            var diagnostics = await this.GetDiagnosticsAsync( _codeWithInstantiatedInheritedAspect, licenseKey );

            if ( accepted )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Contains( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.InheritanceNotAvailable.Id );
            }

            diagnostics = await this.GetDiagnosticsAsync( _codeWithNonInstantiatedInheritedAspect, licenseKey );
            Assert.Empty( diagnostics );
        }
    }
}