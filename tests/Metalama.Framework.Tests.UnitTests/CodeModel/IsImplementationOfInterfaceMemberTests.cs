// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public sealed class IsImplementationOfInterfaceMemberTests : UnitTestClass
{
    [Fact]
    public void ImplicitImplementation()
    {
        var code = """
interface IInterface
{
    int Foo();
    int Foo(int value);
}

class Implementation : IInterface
{
    public int Foo() { return 42; }
    public int Foo(int value) { return 42; }
}
""";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        var interfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "IInterface" );
        var implementationType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Implementation" );

        var interfaceMethod1 = interfaceType.Methods.First();
        var interfaceMethod2 = interfaceType.Methods.Skip( 1 ).First();

        var implementationMethod1 = implementationType.Methods.OfExactSignature( interfaceMethod1 ).AssertNotNull();
        var implementationMethod2 = implementationType.Methods.OfExactSignature( interfaceMethod2 ).AssertNotNull();

        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod1 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod2 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod1 ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod2 ) );
    }

    [Fact]
    public void ExplicitImplementation()
    {
        var code = """
interface IInterface
{
    int Foo();
    int Foo(int value);
}

class Implementation : IInterface
{
    int IInterface.Foo() { return 42; }
    int IInterface.Foo(int value) { return 42; }
}
""";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        var interfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "IInterface" );
        var implementationType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Implementation" );

        var interfaceMethod1 = interfaceType.Methods.Single( m => m.Parameters.Count == 0 );
        var interfaceMethod2 = interfaceType.Methods.Single( m => m.Parameters.Count == 1 );

        var implementationMethod1 = implementationType.Methods.Single( m => m.Parameters.Count == 0 );
        var implementationMethod2 = implementationType.Methods.Single( m => m.Parameters.Count == 1 );

        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod1 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod2 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod1 ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod2 ) );
    }

    [Fact]
    public void Reimplementation()
    {
        var code = """
interface IInterface
{
    int Foo();
    int Foo(int value);
}

class Base : IInterface
{
    public int Foo() { return 42; }
    public int Foo(int value) { return 42; }
}

class Implementation : Base, IInterface
{
    public new int Foo() { return 42; }
    public new int Foo(int value) { return 42; }
}
""";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        var interfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "IInterface" );
        var baseType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Base" );
        var implementationType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Implementation" );

        var interfaceMethod1 = interfaceType.Methods.First();
        var interfaceMethod2 = interfaceType.Methods.Skip( 1 ).First();

        var baseMethod1 = baseType.Methods.OfExactSignature( interfaceMethod1 ).AssertNotNull();
        var baseMethod2 = baseType.Methods.OfExactSignature( interfaceMethod2 ).AssertNotNull();

        var implementationMethod1 = implementationType.Methods.OfExactSignature( interfaceMethod1 ).AssertNotNull();
        var implementationMethod2 = implementationType.Methods.OfExactSignature( interfaceMethod2 ).AssertNotNull();

        Assert.True( implementationType.IsImplementationOfInterfaceMember( baseMethod1, interfaceMethod1 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( baseMethod1, interfaceMethod2 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( baseMethod2, interfaceMethod1 ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( baseMethod2, interfaceMethod2 ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod1 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceMethod2 ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod1 ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceMethod2 ) );
    }

    [Fact]
    public void SubinterfaceReimplementation()
    {
        var code = """
interface ISubinterface
{
    int Foo();
}

interface IInterface : ISubinterface
{
    int Foo(int value);
}

class Base : IInterface
{
    public int Foo() { return 42; }
    public int Foo(int value) { return 42; }
}

class Implementation : Base, ISubinterface
{
    public new int Foo() { return 42; }
}
""";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        var subinterfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "ISubinterface" );
        var interfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "IInterface" );
        var baseType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Base" );
        var implementationType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Implementation" );

        var subinterfaceMethod = subinterfaceType.Methods.First();
        var interfaceMethod = interfaceType.Methods.First();

        var baseMethod1 = baseType.Methods.OfExactSignature( subinterfaceMethod ).AssertNotNull();
        var baseMethod2 = baseType.Methods.OfExactSignature( interfaceMethod ).AssertNotNull();

        var implementationMethod = implementationType.Methods.OfExactSignature( subinterfaceMethod ).AssertNotNull();

        Assert.True( implementationType.IsImplementationOfInterfaceMember( baseMethod1, subinterfaceMethod ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( baseMethod1, interfaceMethod ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( baseMethod2, subinterfaceMethod ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( baseMethod2, interfaceMethod ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod, subinterfaceMethod ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod, interfaceMethod ) );
    }

    [Fact]
    public void GenericImplementation()
    {
        var code = """
interface IInterface<T>
{
    int Foo(T param);
}

class Implementation : IInterface<int>, IInterface<string>
{
    public int Foo(int value) { return 42; }
    public int Foo(string value) { return 42; }
}
""";

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilationModel( code );

        var interfaceType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "IInterface" );
        var interfaceInstanceType1 = (INamedTypeInternal) interfaceType.ConstructGenericInstance( new[] { compilation.Factory.GetSpecialType( SpecialType.Int32 ) } );
        var interfaceInstanceType2 = (INamedTypeInternal) interfaceType.ConstructGenericInstance( new[] { compilation.Factory.GetSpecialType( SpecialType.String ) } );
        var interfaceInstanceType3 = (INamedTypeInternal) interfaceType.ConstructGenericInstance( new[] { compilation.Factory.GetSpecialType( SpecialType.Decimal ) } );
        var implementationType = (INamedTypeInternal) compilation.AllTypes.Single( t => t.Name == "Implementation" );

        var interfaceTypeMethod = interfaceType.Methods.Single();
        var interfaceInstanceType1Method = interfaceInstanceType1.Methods.Single();
        var interfaceInstanceType2Method = interfaceInstanceType2.Methods.Single();
        var interfaceInstanceType3Method = interfaceInstanceType3.Methods.Single();

        var implementationMethod1 = implementationType.Methods.OfExactSignature( interfaceInstanceType1Method ).AssertNotNull();
        var implementationMethod2 = implementationType.Methods.OfExactSignature( interfaceInstanceType2Method ).AssertNotNull();

        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceTypeMethod ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceTypeMethod ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceInstanceType1Method ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceInstanceType2Method ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod1, interfaceInstanceType3Method ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceInstanceType1Method ) );
        Assert.True( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceInstanceType2Method ) );
        Assert.False( implementationType.IsImplementationOfInterfaceMember( implementationMethod2, interfaceInstanceType3Method ) );
    }
}