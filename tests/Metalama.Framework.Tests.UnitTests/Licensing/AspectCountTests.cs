// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Framework.Engine.Licensing;
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
        private const string _tooManyAspectClassesErrorId = LicensingDiagnosticDescriptors.TooManyAspectClassesId;
        private const string _redistributionInvalidErrorId = LicensingDiagnosticDescriptors.RedistributionLicenseInvalidId;
        private const string _noLicenseKeyErrorId = LicensingDiagnosticDescriptors.NoLicenseKeyRegisteredId;

        private readonly ITestOutputHelper _logger;

        public AspectCountTests( ITestOutputHelper logger ) : base( logger )
        {
            this._logger = logger;
        }

        [Theory]
        [InlineData( null, 0, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( null, 1, _arbitraryNamespace, _arbitraryNamespace, _noLicenseKeyErrorId )]
        [InlineData( nameof(TestLicenseKeys.PostSharpFramework), 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( nameof(TestLicenseKeys.PostSharpFramework), 11, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), 3, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), 4, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), 5, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), 6, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaProfessionalBusiness),
            11,
            _arbitraryNamespace,
            _arbitraryNamespace,
            _tooManyAspectClassesErrorId )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution),
            1,
            _arbitraryNamespace,
            _arbitraryNamespace,
            _redistributionInvalidErrorId )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution),
            1,
            _arbitraryNamespace,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            _redistributionInvalidErrorId )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution),
            11,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            _arbitraryNamespace,
            null )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution),
            11,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            null )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound),
            1,
            _arbitraryNamespace,
            _arbitraryNamespace,
            LicensingDiagnosticDescriptors.InvalidLicenseKeyRegisteredId )]
        [InlineData(
            nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound),
            11,
            _arbitraryNamespace,
            _arbitraryNamespace,
            null,
            0,
            TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), 0, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), 2, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), 3, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        public async Task CompilationPassesWithNumberOfAspectsAsync(
            string? licenseKeyName,
            int numberOfAspects,
            string aspectNamespace,
            string targetNamespace,
            string? expectedErrorId,
            int numberOfContracts = 0,
            string projectName = "TestProject" )
        {
            var licenseKey = GetLicenseKey( licenseKeyName );

            const string usingsCode = @"
using Metalama.Framework.Aspects;
using System;
";

            const string aspectPrototype = @"

namespace {0}
{{
    public class Aspect{1} : OverrideMethodAspect
    {{
        public override dynamic? OverrideMethod()
        {{
            System.Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect{1}));
            return meta.Proceed();
        }}
    }}
}}
";

            const string contractPrototype = @"

namespace {0}
{{
    public class Contract{1} : ContractAspect
    {{
        public override void Validate( dynamic? value )
        {{
            if ( value == null )
            {{
                throw new ArgumentNullException(nameof(value), $""Validated by {{nameof(Contract{1})}}."");
            }}
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
        void TargetMethod({2}int? parameter)
        {{
        }}
    }}
}}
";

            this._logger.WriteLine( "License ID:" + (licenseKey == null ? "none" : licenseKey.Split( '-' )[0]) );

            var sourceCodeBuilder = new StringBuilder();
            var aspectApplicationBuilder = new StringBuilder();
            var contractsApplicationBuilder = new StringBuilder();
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

                if ( i < numberOfAspects || numberOfContracts > 0 )
                {
                    aspectOrderApplicationBuilder.Append( ", " );
                }
            }

            for ( var i = 1; i <= numberOfContracts; i++ )
            {
#pragma warning disable SA1114 // Parameter list should follow declaration
                aspectOrderApplicationBuilder.AppendLine(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"typeof({aspectNamespace}.Contract{i}) " );
#pragma warning restore SA1114 // Parameter list should follow declaration

                if ( i < numberOfContracts )
                {
                    aspectOrderApplicationBuilder.Append( ", " );
                }
            }

            sourceCodeBuilder.AppendLine( usingsCode );

            if ( aspectOrderApplicationBuilder.Length > 0 )
            {
                sourceCodeBuilder.AppendLine( 
#if NET
                    CultureInfo.InvariantCulture, 
#endif
                    $"[assembly: AspectOrder( {aspectOrderApplicationBuilder} )]" );
            }

            for ( var i = 1; i <= numberOfAspects; i++ )
            {
                sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, aspectPrototype, aspectNamespace, i ) );
#pragma warning disable SA1114 // Parameter list should follow declaration
                aspectApplicationBuilder.AppendLine(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"[{aspectNamespace}.Aspect{i}]" );
#pragma warning restore SA1114 // Parameter list should follow declaration
            }

            for ( var i = 1; i <= numberOfContracts; i++ )
            {
                sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, contractPrototype, aspectNamespace, i ) );
#pragma warning disable SA1114 // Parameter list should follow declaration
                contractsApplicationBuilder.Append(
#if NET
                    CultureInfo.InvariantCulture,
#endif
                    $"[{aspectNamespace}.Contract{i}]" );
#pragma warning restore SA1114 // Parameter list should follow declaration

                if ( i == numberOfContracts )
                {
                    contractsApplicationBuilder.Append( " " );
                }
            }

            sourceCodeBuilder.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    targetPrototype,
                    targetNamespace,
                    aspectApplicationBuilder.ToString(),
                    contractsApplicationBuilder.ToString() ) );

            var diagnostics = await this.GetDiagnosticsAsync(
                sourceCodeBuilder.ToString(),
                licenseKey,
                aspectNamespace,
                projectName );

            if ( expectedErrorId == null )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == expectedErrorId );
            }
            
            if ( expectedErrorId == null && numberOfAspects == 0 && numberOfContracts == 0 )
            {
                Assert.False( this.ToastNotifications.WasDetectionTriggered );
            }
            else
            {
                Assert.True( this.ToastNotifications.WasDetectionTriggered );
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
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Fact]
        public async Task SkippedAspectsDontCountAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect1));
        return meta.Proceed();
    }
}

public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

public class Aspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect3));
        return meta.Proceed();
    }
}

public class SkippedAspect : OverrideMethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.SkipAspect();
    }

    public override dynamic? OverrideMethod()
    {
        throw new InvalidOperationException();
    }
}

class TargetClass
{
    [Aspect1]
    [Aspect2]
    [Aspect3]
    [SkippedAspect]
    void TargetMethod()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

            Assert.Empty( diagnostics );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Fact]
        public async Task PartiallySkippedAspectsDoCountAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect1));
        return meta.Proceed();
    }
}

public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

public class Aspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

public class OptionallySkippedAspect : OverrideMethodAspect
{
    private readonly bool _isSkipped;

    public OptionallySkippedAspect(bool isSkipped)
    {
        this._isSkipped = isSkipped;
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        if (this._isSkipped)
        {
            builder.SkipAspect();
        }
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(OptionallySkippedAspect));
        return meta.Proceed();
    }
}

class TargetClass
{
    [Aspect1]
    [Aspect2]
    [Aspect3]
    [OptionallySkippedAspect(false)]
    void TargetMethod1()
    {
    }

    [OptionallySkippedAspect(true)]
    void TargetMethod2()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

            Assert.Single( diagnostics, d => d.Id == _tooManyAspectClassesErrorId );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Fact]
        public async Task AbstractAspectInstancesDontCountAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obfuscation( Exclude = true )]
    internal static class IsExternalInit { }
}

[Inheritable]
public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect1));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect3));
        return meta.Proceed();
    }
}

public class ConditionallyInheritableAspect1 : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    bool IConditionallyInheritableAspect.IsInheritable(IDeclaration targetDeclaration, IAspectInstance aspectInstance) => IsInheritable;

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(ConditionallyInheritableAspect1));
        return meta.Proceed();
    }
}

interface ITargetInterface
{
    [Aspect1]
    [Aspect2]
    [Aspect3]
    [ConditionallyInheritableAspect1(IsInheritable = true)]
    void TargetMethod();
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

            Assert.Empty( diagnostics );
            Assert.False( this.ToastNotifications.WasDetectionTriggered );
        }

        [Theory]
        [InlineData( null, _noLicenseKeyErrorId )]
        [InlineData( nameof(TestLicenseKeys.PostSharpFramework), null )]
        [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), _tooManyAspectClassesErrorId )]
        [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), null )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), null )]
        public async Task InheritedAspectsCountAsync( string licenseKeyName, string? expectedErrorId )
        {
            var licenseKey = GetLicenseKey( licenseKeyName );

            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obfuscation( Exclude = true )]
    internal static class IsExternalInit { }
}

[Inheritable]
public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect1));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect3));
        return meta.Proceed();
    }
}

public class ConditionallyInheritableAspect1 : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    bool IConditionallyInheritableAspect.IsInheritable(IDeclaration targetDeclaration, IAspectInstance aspectInstance) => IsInheritable;

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(ConditionallyInheritableAspect1));
        return meta.Proceed();
    }
}

interface ITargetInterface
{
    [Aspect1]
    [Aspect2]
    [Aspect3]
    [ConditionallyInheritableAspect1(IsInheritable = true)]
    void TargetMethod();
}

class TargetClass : ITargetInterface
{
    public void TargetMethod()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, licenseKey );

            if ( expectedErrorId == null )
            {
                Assert.Empty( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == expectedErrorId );
            }
            
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Fact]
        public async Task ExcludedAspectInstancesDontCountAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

[Inheritable]
public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect1));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect2));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect3));
        return meta.Proceed();
    }
}

[Inheritable]
public class Aspect4 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + "" enhanced by "" + nameof(Aspect4));
        return meta.Proceed();
    }
}

interface ITargetInterface
{
    [Aspect1]
    [Aspect2]
    [Aspect3]
    [Aspect4]
    void TargetMethod();
}

class TargetClass : ITargetInterface
{
    [ExcludeAspect(typeof(Aspect4))]
    public void TargetMethod()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

            Assert.Empty( diagnostics );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }

        [Fact]
        public async Task NotificationsAreTriggeredWhenOnlyAspectsAreUsedAsync()
        {
            const string code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
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
    void TargetMethod()
    {
    }
}
";

            var diagnostics = await this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaUltimateBusiness );

            Assert.Empty( diagnostics );
            Assert.True( this.ToastNotifications.WasDetectionTriggered );
        }
    }
}