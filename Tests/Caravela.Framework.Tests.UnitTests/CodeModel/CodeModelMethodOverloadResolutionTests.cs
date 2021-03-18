// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelMethodOverloadResolutionTests : TestBase
    {
        [Fact]
        public void Single_Void_NonGeneric_NoParam_Match()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Foo", 0, Array.Empty<Code.IType>() ).SingleOrDefault();

            Assert.Same( type.Methods[0], matchedMethod );
        }

        [Fact]
        public void Single_Void_NonGeneric_SingleParam_Match()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { compilation.Factory.GetTypeByReflectionType( typeof( int ) ) } ).SingleOrDefault();

            Assert.Same( type.Methods[1], matchedMethod );
        }

        [Fact]
        public void Single_Void_Generic_NoParam_Match()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Foo", 1, Array.Empty<Code.IType>() ).SingleOrDefault();

            Assert.Same( type.Methods[2], matchedMethod );
        }

        [Fact]
        public void Single_Void_NonGeneric_SingleParam_NameDoesNotMatch()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Bar", 0, new[] { compilation.Factory.GetTypeByReflectionType( typeof( int ) ) } ).SingleOrDefault();

            Assert.Null( matchedMethod );
        }

        [Fact]
        public void Single_Void_NonGeneric_SingleParams_ParamTypeDoesNotMatch()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Bar", 0, new[] { compilation.Factory.GetTypeByReflectionType( typeof( string ) ) } ).SingleOrDefault();

            Assert.Null( matchedMethod );
        }

        [Fact]
        public void Single_Void_NonGeneric_TwoParams_ParamCountDoesNotMatch()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( 
                "Bar", 
                0, 
                new[] 
                { 
                    compilation.Factory.GetTypeByReflectionType( typeof( int ) ), 
                    compilation.Factory.GetTypeByReflectionType( typeof( int ) ) 
                } ).SingleOrDefault();

            Assert.Null( matchedMethod );
        }

        [Fact]
        public void Single_Void_NonGeneric_TwoParams_GenericParamCountDoesNotMatch()
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

    public void Foo<T>()
    {
    }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes[0];
            var matchedMethod = type.Methods.OfCompatibleSignature( "Foo", 2, Array.Empty<Code.IType>() ).SingleOrDefault();

            Assert.Null( matchedMethod );
        }
    }
}
