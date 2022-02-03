// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Introspection;
using Metalama.TestFramework;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Introspection;

public class IntrospectionTests : TestBase
{
    [Fact]
    public void Success()
    {
        var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

class Aspect : TypeAspect
{

  private static readonly DiagnosticDefinition _warning = new(
            ""MY001"",
        Severity.Warning,
        ""Message"" );


    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        _warning.ReportTo( builder.Diagnostics );
    }

    [Introduce]
    public void NewMethod(){}

}

[Aspect]
class MyClass
{
    
    void MyMethod() {}
}
";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        using var domain = new UnloadableCompileTimeDomain();
        var compiler = new IntrospectionCompiler( domain, true );
        var compilerOutput = compiler.Compile( compilation, testContext.ServiceProvider );

        Assert.True( compilerOutput.IsSuccessful );
        Assert.Single( compilerOutput.Diagnostics );
        Assert.Single( compilerOutput.AspectInstances );
        Assert.Single( compilerOutput.AspectInstances[0].Diagnostics );
        Assert.Single( compilerOutput.AspectInstances[0].IntroducedMembers );
        var aspectClass = compilerOutput.AspectClasses.Single( x => x.ShortName == "Aspect" );
        Assert.Same( compilerOutput.AspectInstances[0], aspectClass.Instances[0] );
    }

    [Fact]
    public void UserError()
    {
        var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

class Aspect : TypeAspect
{

  private static readonly DiagnosticDefinition _error = new(
            ""MY001"",
        Severity.Error,
        ""Message"" );


    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        _error.ReportTo( builder.Diagnostics );
    }

    [Introduce]
    public void NewMethod(){}

}

[Aspect]
class MyClass
{
    
    void MyMethod() {}
}
";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        using var domain = new UnloadableCompileTimeDomain();
        var compiler = new IntrospectionCompiler( domain, true );
        var compilerOutput = compiler.Compile( compilation, testContext.ServiceProvider );

        Assert.False( compilerOutput.IsSuccessful );
        Assert.Single( compilerOutput.Diagnostics );
        Assert.Single( compilerOutput.AspectInstances );
        Assert.Single( compilerOutput.AspectInstances[0].Diagnostics );
        Assert.Empty( compilerOutput.AspectInstances[0].IntroducedMembers );
    }

    [Fact]
    public void SyntaxError()
    {
        var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

class Aspect : TypeAspect
{
    xxx
}

 

    [Introduce]
    public void NewMethod(){}

}

[Aspect]
class MyClass
{
    
    void MyMethod() {}
}
";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

        using var domain = new UnloadableCompileTimeDomain();
        var compiler = new IntrospectionCompiler( domain, true );
        var compilerOutput = compiler.Compile( compilation, testContext.ServiceProvider );

        Assert.False( compilerOutput.IsSuccessful );
        Assert.NotEmpty( compilerOutput.Diagnostics );
        Assert.Single( compilerOutput.AspectInstances );
    }
}