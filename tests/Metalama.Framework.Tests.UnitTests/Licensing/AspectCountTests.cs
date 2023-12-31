﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
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
        private const string _tooManyAspectClassesErrorId = "LAMA0800";
        private const string _redistributionInvalidErrorId = "LAMA0803";
        private const string _noLicenseKeyErrorId = "LAMA0809";

        private readonly ITestOutputHelper _logger;

        public AspectCountTests( ITestOutputHelper logger ) : base( logger )
        {
            this._logger = logger;
        }

        [Theory]
        [TestLicensesInlineData( null, 1, _arbitraryNamespace, _arbitraryNamespace, _noLicenseKeyErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.PostSharpEssentials ), 1, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.PostSharpFramework ), 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.PostSharpFramework ), 11, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.PostSharpUltimate ), 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaFreePersonal ), 3, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaFreePersonal ), 4, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaStarterBusiness ), 5, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaStarterBusiness ), 6, _arbitraryNamespace, _arbitraryNamespace, _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaProfessionalBusiness ), 10, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaProfessionalBusiness ),
            11,
            _arbitraryNamespace,
            _arbitraryNamespace,
            _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaUltimateBusiness ), 11, _arbitraryNamespace, _arbitraryNamespace, null )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution ),
            1,
            _arbitraryNamespace,
            _arbitraryNamespace,
            _redistributionInvalidErrorId )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution ),
            1,
            _arbitraryNamespace,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            _redistributionInvalidErrorId )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution ),
            11,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            _arbitraryNamespace,
            null )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution ),
            11,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            TestLicenseKeys.MetalamaUltimateRedistributionNamespace,
            null )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimatePersonalProjectBound ),
            1,
            _arbitraryNamespace,
            _arbitraryNamespace,
            _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData(
            nameof( TestLicenseKeys.MetalamaUltimatePersonalProjectBound ),
            11,
            _arbitraryNamespace,
            _arbitraryNamespace,
            null,
            0,
            TestLicenseKeys.MetalamaUltimateProjectBoundProjectName )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaFreePersonal ), 0, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaFreePersonal ), 2, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        [TestLicensesInlineData( nameof( TestLicenseKeys.MetalamaFreePersonal ), 3, _arbitraryNamespace, _arbitraryNamespace, null, 4 )]
        public async Task CompilationPassesWithNumberOfAspectsAsync(
            string? licenseKey,
            int numberOfAspects,
            string aspectNamespace,
            string targetNamespace,
            string? expectedErrorId,
            int numberOfContracts = 0,
            string projectName = "TestProject" )
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

            sourceCodeBuilder.AppendLine( string.Format( CultureInfo.InvariantCulture, usingsAndOrdering, aspectOrderApplicationBuilder.ToString() ) );

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

            var diagnostics = await this.GetDiagnosticsAsync( sourceCodeBuilder.ToString(), licenseKey, aspectNamespace, projectName );

            if ( expectedErrorId == null )
            {
                AssertEmptyOrSdkOnly( diagnostics );
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

            AssertEmptyOrSdkOnly( diagnostics );
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

            AssertEmptyOrSdkOnly( diagnostics );
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

            AssertEmptyOrSdkOnly( diagnostics );
        }

        [Theory]
        [TestLicensesInlineData( "None", null, _noLicenseKeyErrorId )]
        [TestLicensesInlineData( "PostSharp Essentials", nameof( TestLicenseKeys.PostSharpEssentials ), _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( "PostSharp Framework", nameof( TestLicenseKeys.PostSharpFramework ), null )]
        [TestLicensesInlineData( "PostSharp Ultimate", nameof( TestLicenseKeys.PostSharpUltimate ), null )]
        [TestLicensesInlineData( "Metalama Free Personal", nameof( TestLicenseKeys.MetalamaFreePersonal ), _tooManyAspectClassesErrorId )]
        [TestLicensesInlineData( "Metalama Starter Business", nameof( TestLicenseKeys.MetalamaStarterBusiness ), null )]
        [TestLicensesInlineData( "Metalama Professional Business", nameof( TestLicenseKeys.MetalamaProfessionalBusiness ), null )]
        [TestLicensesInlineData( "Metalama Ultimate Business", nameof( TestLicenseKeys.MetalamaUltimateBusiness ), null )]
        public async Task InheritedAspectsCountAsync( string licenseKeyName, string licenseKey, string? expectedErrorId )
        {
            _ = licenseKeyName;

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
                AssertEmptyOrSdkOnly( diagnostics );
            }
            else
            {
                Assert.Single( diagnostics, d => d.Id == expectedErrorId );
            }
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

            AssertEmptyOrSdkOnly( diagnostics );
        }
    }
}