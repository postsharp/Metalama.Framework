// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CA1305 // The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. 

namespace Metalama.Framework.Tests.UnitTests.Licensing;

public sealed class ChildAspectsCountTests : LicensingTestsBase
{
    private const string _namespace = "ChildAspectsCountTests";
    
    private const string _overrideMethodChildAspectName = "OverrideMethodChildAspect";
    
    private const string _iAspectChildAspectName = "IAspectChildAspect";

    private const string _aspectUsingsCode = $@"using {_namespace};
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;
using System.Linq;
";
    
    private const string _overrideMethodChildAspectCodeFormat = @$"namespace {_namespace} {{{{
    internal class {_overrideMethodChildAspectName}{{0}} : OverrideMethodAspect
    {{{{
        public override dynamic? OverrideMethod()
        {{{{
            var predecessors = string.Join("";"", meta.AspectInstance.Predecessors.Select(p => $""{{{{(p.Instance is IAspectInstance aspectinstance ? aspectinstance.AspectClass.ShortName : ""unknown"")}}}}/{{{{p.Kind}}}}""));
            Console.WriteLine($""Method with a {{{{this.GetType().Name}}}}. Predecessors: {{{{predecessors}}}}"");
            return meta.Proceed();
        }}}}
    }}}}
}}}}
";
    
    private const string _iAspectChildAspectCodeFormat = @$"namespace {_namespace} {{{{
    {{0}} class {_iAspectChildAspectName}{{1}} : IAspect<IMethod>
    {{{{
        public void BuildAspect(IAspectBuilder<IMethod> builder)
        {{{{
            builder.Advice.Override(builder.Target, nameof(OverrideMethod));
        }}}}

        public void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {{{{
        }}}}

        [Template]
        public dynamic? OverrideMethod()
        {{{{
            var predecessors = string.Join("";"", meta.AspectInstance.Predecessors.Select(p => $""{{{{(p.Instance is IAspectInstance aspectinstance ? aspectinstance.AspectClass.ShortName : ""unknown"")}}}}/{{{{p.Kind}}}}""));
            Console.WriteLine($""Method with a {{{{this.GetType().Name}}}}. Predecessors: {{{{predecessors}}}}"");
            return meta.Proceed();
        }}}}
    }}}}
}}}}
";
    
    private const string _parentAspectCodeFormat = @$"namespace {_namespace} {{{{
    public class ParentAspect : TypeAspect
    {{{{
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {{{{
            builder.Outbound.SelectMany(m => m.AllMethods).Where(m => m.Name.StartsWith(""Method"")).AddAspect<{{0}}0>();
            builder.Outbound.SelectMany(m => m.AllMethods).Where(m => m.Name.StartsWith(""Method"")).AddAspect<{{0}}1>();
            builder.Outbound.SelectMany(m => m.AllMethods).Where(m => m.Name.StartsWith(""Method"")).AddAspect<{{0}}2>();
        }}}}
    }}}}
}}}}
";
    
    private const string _targetClassCode = @$"namespace {_namespace} {{
    [ParentAspect]
    internal class Class1
    {{
        public void Method1()
        {{
        }}

        public void Method2()
        {{
        }}

        public void Method3()
        {{
        }}

        public void Method4()
        {{
        }}
    }}
}}
";

    private const string _fabricsUsingsCode = @"using Metalama.Framework.Fabrics;
";
    
    private const string _fabricCodeFormat = @$"namespace {_namespace} {{{{
    internal class Fabric : ProjectFabric
    {{{{
        public override void AmendProject(IProjectAmender amender)
        {{{{
            amender
                .Outbound
                .SelectMany(p => p.AllTypes)
                .Where(t => t.Name == nameof(Class1))
                .SelectMany(t => t.AllMethods)
                .Where(m => m.Name.StartsWith(""Method""))
                .AddAspect<{{0}}0>();

            amender
                .Outbound
                .SelectMany(p => p.AllTypes)
                .Where(t => t.Name == nameof(Class1))
                .SelectMany(t => t.AllMethods)
                .Where(m => m.Name.StartsWith(""Method""))
                .AddAspect<{{0}}1>();

            amender
                .Outbound
                .SelectMany(p => p.AllTypes)
                .Where(t => t.Name == nameof(Class1))
                .SelectMany(t => t.AllMethods)
                .Where(m => m.Name.StartsWith(""Method""))
                .AddAspect<{{0}}2>();
        }}}}
    }}}}
}}}}
";
    
    public ChildAspectsCountTests( ITestOutputHelper logger ) : base( logger ) { }

    private Task<DiagnosticBag> GetDiagnosticsWithFreeLicenseAsync( string code ) => this.GetDiagnosticsAsync( code, TestLicenseKeys.MetalamaFreePersonal );

    private void AssertTooManyAspectClasses( DiagnosticBag diagnostics )
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.TooManyAspectClasses.Id );
        Assert.True( this.ToastNotifications.WasDetectionTriggered );
    }

    [Fact]
    public async Task CompilationPassesWhenChildAspectsAreNotAttributesAsync()
    {
        var code = _aspectUsingsCode
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "0" )
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "1" )
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "2" )
                   + string.Format( _parentAspectCodeFormat, _iAspectChildAspectName ) 
                   + _targetClassCode;

        var diagnostics = await this.GetDiagnosticsWithFreeLicenseAsync( code );
        
        Assert.Empty( diagnostics );
        Assert.True( this.ToastNotifications.WasDetectionTriggered );
    }
    
    [Fact]
    public async Task CompilationFailsWhenChildAspectsAreAttributesAsync()
    {
        var code = _aspectUsingsCode
                   + string.Format( _overrideMethodChildAspectCodeFormat, "0" )
                   + string.Format( _overrideMethodChildAspectCodeFormat, "1" )
                   + string.Format( _overrideMethodChildAspectCodeFormat, "2" )
                   + string.Format( _parentAspectCodeFormat, _overrideMethodChildAspectName )
                   + _targetClassCode;

        var diagnostics = await this.GetDiagnosticsWithFreeLicenseAsync( code );

        this.AssertTooManyAspectClasses( diagnostics );
    }
    
    [Fact]
    public async Task CompilationFailsWhenChildAspectsArePublicAsync()
    {
        var code = _aspectUsingsCode
                   + string.Format( _iAspectChildAspectCodeFormat, "public", "0" )
                   + string.Format( _iAspectChildAspectCodeFormat, "public", "1" )
                   + string.Format( _iAspectChildAspectCodeFormat, "public", "2" )
                   + string.Format( _parentAspectCodeFormat, _iAspectChildAspectName ) 
                   + _targetClassCode;

        var diagnostics = await this.GetDiagnosticsWithFreeLicenseAsync( code );

        this.AssertTooManyAspectClasses( diagnostics );
    }
    
    [Fact]
    public async Task CompilationFailsWhenChildAspectsAreAppliedUsingFabricsAsync()
    {
        var code = _aspectUsingsCode
                   + _fabricsUsingsCode
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "0" )
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "1" )
                   + string.Format( _iAspectChildAspectCodeFormat, "internal", "2" )
                   + string.Format( _parentAspectCodeFormat, _iAspectChildAspectName )
                   + _targetClassCode
                   + string.Format( _fabricCodeFormat, _iAspectChildAspectName );

        var diagnostics = await this.GetDiagnosticsWithFreeLicenseAsync( code );

        Assert.Single( diagnostics, d => d.Id == LicensingDiagnosticDescriptors.FabricsNotAvailable.Id );
        Assert.True( this.ToastNotifications.WasDetectionTriggered );
    }
}