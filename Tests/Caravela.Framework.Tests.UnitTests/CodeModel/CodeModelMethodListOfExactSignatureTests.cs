// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", 0, Array.Empty<Code.IType>() );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType } );
            Assert.Same( type.Methods[1], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType, intType } );
            Assert.Same( type.Methods[2], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType, intType, intType } );
            Assert.Null( matchedMethod4 );
        }

        [Fact]
        public void Matches_ParameterType()
        {
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof( object ) );
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );
            var stringType = compilation.Factory.GetTypeByReflectionType( typeof( string ) );
            var intArrayType = compilation.Factory.GetTypeByReflectionType( typeof( int[] ) );
            var objectArrayType = compilation.Factory.GetTypeByReflectionType( typeof( string[] ) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", 0, new[] { objectType } );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType } );
            Assert.Same( type.Methods[1], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", 0, new[] { stringType } );
            Assert.Same( type.Methods[2], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Foo", 0, new[] { intArrayType } );
            Assert.Same( type.Methods[3], matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Foo", 0, new[] { objectArrayType } );
            Assert.Null( matchedMethod5 );
        }

        [Fact]
        public void Matches_GenericParameterCount()
        {
            var code = @"
class C
{
    public void Foo(int x)
    {
    }

    public void Foo<T1>(int x)
    {
    }

    public void Foo<T1,T2>(int x)
    {
    }

    public void Foo<T1,T2,T3>(int x)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType } );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", 1, new[] { intType } );
            Assert.Same( type.Methods[1], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", 2, new[] { intType } );
            Assert.Same( type.Methods[2], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Foo", 3, new[] { intType } );
            Assert.Same( type.Methods[3], matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Foo", 4, new[] { intType } );
            Assert.Null( matchedMethod5 );
        }

        [Fact]
        public void Matches_RefKind()
        {
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType } );
            Assert.Same( type.Methods[0], matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType }, new[] { RefKind.None } );
            Assert.Same( type.Methods[0], matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType }, new[] { RefKind.Ref } );
            Assert.Same( type.Methods[1], matchedMethod3 );
            var matchedMethod4 = type.Methods.OfExactSignature( "Bar", 0, new[] { intType }, new[] { RefKind.In } );
            Assert.Same( type.Methods[2], matchedMethod4 );
            var matchedMethod5 = type.Methods.OfExactSignature( "Quz", 0, new[] { intType }, new[] { RefKind.Out } );
            Assert.Same( type.Methods[3], matchedMethod5 );
            var matchedMethod6 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType }, new[] { RefKind.In } );
            Assert.Null( matchedMethod6 );
            var matchedMethod7 = type.Methods.OfExactSignature( "Bar", 0, new[] { intType }, new[] { RefKind.Out } );
            Assert.Null( matchedMethod7 );
            var matchedMethod8 = type.Methods.OfExactSignature( "Quz", 0, new[] { intType }, new[] { RefKind.Ref } );
            Assert.Null( matchedMethod8 );
        }

        [Fact]
        public void DeclaredOnlyTrue_Ignores_BaseMethod()
        {
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[2];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethod1 = type.Methods.OfExactSignature( "Foo", 0, Array.Empty<Code.IType>() );
            Assert.Null( matchedMethod1 );
            var matchedMethod2 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType } );
            Assert.Null( matchedMethod2 );
            var matchedMethod3 = type.Methods.OfExactSignature( "Foo", 0, new[] { intType, intType } );
            Assert.Same( type.Methods[0], matchedMethod3 );
        }

        [Fact]
        public void DeclaredOnlyFalse_Finds_BaseMethod()
        {
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

            var compilation = CreateCompilation( code );
            var typeA = compilation.DeclaredTypes[0];
            var typeB = compilation.DeclaredTypes[1];
            var typeC = compilation.DeclaredTypes[2];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethod1 = typeC.Methods.OfExactSignature( "Foo", 0, Array.Empty<Code.IType>(), declaredOnly: false );
            Assert.Same( typeA.Methods[0], matchedMethod1 );
            var matchedMethod2 = typeC.Methods.OfExactSignature( "Foo", 0, new[] { intType }, declaredOnly: false );
            Assert.Same( typeB.Methods[0], matchedMethod2 );
            var matchedMethod3 = typeC.Methods.OfExactSignature( "Foo", 0, new[] { intType, intType }, declaredOnly: false );
            Assert.Same( typeC.Methods[0], matchedMethod3 );
        }
    }
}
