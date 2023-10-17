// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
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
        [TestLicensesInlineData( "PostSharp Essentials", nameof(TestLicenses.PostSharpEssentials), false )]
        [TestLicensesInlineData( "PostSharp Framework", nameof(TestLicenses.PostSharpFramework), true )]
        [TestLicensesInlineData( "PostSharp Ultimate", nameof(TestLicenses.PostSharpUltimate), true )]
        [TestLicensesInlineData( "Metalama Free Personal", nameof(TestLicenses.MetalamaFreePersonal), false )]
        [TestLicensesInlineData( "Metalama Starter Business", nameof(TestLicenses.MetalamaStarterBusiness), true )]
        [TestLicensesInlineData( "Metalama Professional Business", nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
        [TestLicensesInlineData( "Metalama Ultimate Business", nameof(TestLicenses.MetalamaUltimateBusiness), true )]
        public async Task InheritanceIsAcceptedAsync( string licenseName, string licenseKey, bool accepted )
        {
            _ = licenseName;

            var diagnostics = await this.GetDiagnosticsAsync( _codeWithInstantiatedInheritedAspect, licenseKey );

            if ( accepted )
            {
                // We want to assert that the diagnostics are empty, but unit tests reference Metalama.Framework.Sdk,
                // so we need to ignore the Roslyn API license error.
                if ( diagnostics.Count > 0 )
                {
                    Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.RoslynApiNotAvailable.Id );
                }
            }
            else
            {
                Assert.Contains( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.InheritanceNotAvailable.Id );
            }

            diagnostics = await this.GetDiagnosticsAsync( _codeWithNonInstantiatedInheritedAspect, licenseKey );

            // We want to assert that the diagnostics are empty, but unit tests reference Metalama.Framework.Sdk,
            // so we need to ignore the Roslyn API license error.
            if ( diagnostics.Count > 0 )
            {
                Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.RoslynApiNotAvailable.Id );
            }
        }
    }
}