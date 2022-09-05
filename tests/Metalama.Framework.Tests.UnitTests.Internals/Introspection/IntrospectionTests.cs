// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
builder.Diagnostics.Report(         _warning );
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
        var aspectInstances = compilerOutput.AspectInstances;
        Assert.Single( aspectInstances );
        Assert.Single( aspectInstances[0].Diagnostics );
        Assert.Single( aspectInstances[0].Advice );
        var aspectClass = compilerOutput.AspectClasses.Single( x => x.ShortName == "Aspect" );
        Assert.Same( aspectInstances[0], aspectClass.Instances[0] );
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
builder.Diagnostics.Report(         _error );
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
    }

    [Fact]
    public void SyntaxErrorInCompileTimeCode()
    {
        var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

class Aspect : TypeAspect
{
    public override BuildAspect( IAspectBuilder<INamedType> builder )
   {
        some_error;
   }
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
    }
}