// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Tests.UnitTests.Utilities;
using System;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelMethodListOfExactSignatureTests : TestBase
    {
        [Fact]
        public void Matches_ParameterCount()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class C
{
    public void Foo()
    {
    }

    public void Foo(int x)
    {
    }

    public void Foo(int x, int y)
    {
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", Array.Empty<IType>() );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", new[] { intType } );
            Assert.Same( type.Methods[1], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", new[] { intType, intType } );
            Assert.Same( type.Methods[2], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Foo", new[] { intType, intType, intType } );
            Assert.Null( matchedMethod4 );
        }

        [Fact]
        public void Matches_ParameterType()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class C
{
    public void Foo(object x)
    {
    }

    public void Foo(int x)
    {
    }

    public void Foo(string x)
    {
    }

    public void Foo(int[] x)
    {
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof(object) );
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );
            var stringType = compilation.Factory.GetTypeByReflectionType( typeof(string) );
            var intArrayType = compilation.Factory.GetTypeByReflectionType( typeof(int[]) );
            var objectArrayType = compilation.Factory.GetTypeByReflectionType( typeof(string[]) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", new[] { objectType } );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", new[] { intType } );
            Assert.Same( type.Methods[1], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", new[] { stringType } );
            Assert.Same( type.Methods[2], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Foo", new[] { intArrayType } );
            Assert.Same( type.Methods[3], matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Foo", new[] { objectArrayType } );
            Assert.Null( matchedMethod5 );
        }

        [Fact]
        public void Matches_RefKind()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class C
{
    public void Foo(int x)
    {
    }

    public void Foo(ref int x)
    {
    }

    public void Bar(in int x)
    {
    }

    public void Quz(out int x)
    {
        x = 42;
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", new[] { intType } );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", new[] { intType }, new[] { RefKind.None } );
            Assert.Same( type.Methods[0], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", new[] { intType }, new[] { RefKind.Ref } );
            Assert.Same( type.Methods[1], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Bar", new[] { intType }, new[] { RefKind.In } );
            Assert.Same( type.Methods[2], matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Quz", new[] { intType }, new[] { RefKind.Out } );
            Assert.Same( type.Methods[3], matchedMethod5 );
            var matchedMethod6 = type.Methods.OfExactSignature( "Foo", new[] { intType }, new[] { RefKind.In } );
            Assert.Null( matchedMethod6 );
            var matchedMethod7 = type.Methods.OfExactSignature( "Bar", new[] { intType }, new[] { RefKind.Out } );
            Assert.Null( matchedMethod7 );
            var matchedMethod8 = type.Methods.OfExactSignature( "Quz", new[] { intType }, new[] { RefKind.Ref } );
            Assert.Null( matchedMethod8 );
        }

        [Fact]
        public void DeclaredOnlyTrue_Ignores_BaseMethod()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class A
{
    public void Foo()
    {
    }
}

class B : A
{
    public void Foo(int x)
    {
    }
}

class C : B
{
    public void Foo(int x, int y)
    {
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var types = compilation.Types.OrderBySource();
            var type = types[2];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", Array.Empty<IType>() );
            Assert.Null( matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", new[] { intType } );
            Assert.Null( matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", new[] { intType, intType } );
            Assert.Same( type.Methods[0], matchedMethod3 );
        }

        [Fact]
        public void DeclaredOnlyFalse_Finds_BaseMethod()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class A
{
    public void Foo()
    {
    }
}

class B : A
{
    public void Foo(int x)
    {
    }
}

class C : B
{
    public void Foo(int x, int y)
    {
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var types = compilation.Types.OrderBySource();
            var typeA = types[0];
            var typeB = types[1];
            var typeC = types[2];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethod1 = typeC.Methods.OfExactSignature( "Foo", Array.Empty<IType>(), declaredOnly: false );
            Assert.Same( typeA.Methods.Single(), matchedMethod1 );
            var matchedMethod2 = typeC.Methods.OfExactSignature( "Foo", new[] { intType }, declaredOnly: false );
            Assert.Same( typeB.Methods.Single(), matchedMethod2 );
            var matchedMethod3 = typeC.Methods.OfExactSignature( "Foo", new[] { intType, intType }, declaredOnly: false );
            Assert.Same( typeC.Methods.Single(), matchedMethod3 );
        }

        [Fact]
        public void DeclaredOnlyFalse_Finds_BaseMethod_GenericDeclaringType()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class A : System.Collections.Generic.List<int> { }
";

            var compilation = testContext.CreateCompilationModel( code );
            var typeA = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethod1 = typeA.Methods.OfExactSignature( "Add", new[] { intType }, declaredOnly: false );

            Assert.NotNull( matchedMethod1 );
            Assert.Equal( "List", matchedMethod1!.DeclaringType.Name );
        }

        [Fact]
        public void Matches_IsStatic()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class C
{
    public void Foo()
    {
    }

    public static void Bar()
    {
    }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", Array.Empty<IType>(), isStatic: false );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", Array.Empty<IType>(), isStatic: true );
            Assert.Null( matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", Array.Empty<IType>(), isStatic: null );
            Assert.Same( type.Methods[0], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Bar", Array.Empty<IType>(), isStatic: false );
            Assert.Null( matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Bar", Array.Empty<IType>(), isStatic: true );
            Assert.Same( type.Methods[1], matchedMethod5 );
            var matchedMethod6 = type.Methods.OfExactSignature( "Bar", Array.Empty<IType>(), isStatic: null );
            Assert.Same( type.Methods[1], matchedMethod6 );
        }
    }
}