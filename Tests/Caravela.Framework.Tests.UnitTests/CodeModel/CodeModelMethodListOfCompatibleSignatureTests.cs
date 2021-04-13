// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelMethodListOfCompatibleSignatureTests : TestBase
    {
        [Fact]
        public void Matches_ParameterCount()
        {
            var code = @"
class C
{
    public void Bar()
    {
    }

    public void Foo()
    {
    }

    public void Bar(int x)
    {
    }

    public void Foo(int x)
    {
    }

    public void Bar(int x, int y)
    {
    }

    public void Foo(int x, int y)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo" );
            Assert.Equal( new[] { type.Methods[1], type.Methods[3], type.Methods[5] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Bar" );
            Assert.Equal( new[] { type.Methods[0], type.Methods[2], type.Methods[4] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Quz" );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods3 );
        }

        [Fact]
        public void Matches_GenericParameterCount()
        {
            var code = @"
class C
{
    public void Foo()
    {
    }

    public void Foo<T1>()
    {
    }

    public void Foo<T1,T2>()
    {
    }

    public void Foo(int x)
    {
    }

    public void Foo<T1>(int x)
    {
    }

    public void Foo<T1,T2>(int x)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", 0 );
            Assert.Equal( new[] { type.Methods[0], type.Methods[3] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", 1 );
            Assert.Equal( new[] { type.Methods[1], type.Methods[4] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", 2 );
            Assert.Equal( new[] { type.Methods[2], type.Methods[5] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", 3 );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods4 );
        }

        [Fact]
        public void Matches_ParameterTypes()
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

    public void Foo(object x)
    {
    }

    public void Foo(int x, int y)
    {
    }

    public void Foo(object x, object y)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof( object ) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: Array.Empty<IType>() );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType } );
            Assert.Equal( new[] { type.Methods[1], type.Methods[2] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType } );
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType, intType } );
            Assert.Equal( new[] { type.Methods[3], type.Methods[4] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, objectType } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType, objectType } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, intType } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, objectType, objectType } );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods8 );
        }

        [Fact]
        public void Matches_ParameterTypeRefKinds()
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

    public void Bar(int x)
    {
    }

    public void Bar(out int x)
    {
        x = 0;
    }

    public void Quz(int x)
    {
    }

    public void Quz(in int x)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { intType } );
            Assert.Equal( new[] { type.Methods[0], type.Methods[1] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[0], type.Methods[1] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { intType }, new RefKind?[] { RefKind.Ref } );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { intType }, new RefKind?[] { RefKind.RefReadOnly } );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Bar", 0, new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[2], type.Methods[3] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Bar", 0, new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Bar", 0, new[] { intType }, new RefKind?[] { RefKind.Out } );
            Assert.Equal( new[] { type.Methods[3] }, matchedMethods8 );
            var matchedMethods9 = type.Methods.OfCompatibleSignature( "Quz", 0, new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[4], type.Methods[5] }, matchedMethods9 );
            var matchedMethods10 = type.Methods.OfCompatibleSignature( "Quz", 0, new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods10 );
            var matchedMethods11 = type.Methods.OfCompatibleSignature( "Quz", 0, new[] { intType }, new RefKind?[] { RefKind.In } );
            Assert.Equal( new[] { type.Methods[5] }, matchedMethods11 );
        }

        [Fact]
        public void Matches_ParameterReflectionTypes()
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

    public void Foo(object x)
    {
    }

    public void Foo(int x, int y)
    {
    }

    public void Foo(object x, object y)
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", 0, Array.Empty<IType>() );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { typeof( int ) } );
            Assert.Equal( new[] { type.Methods[1], type.Methods[2] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { typeof( object ) } );
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { typeof( int ), typeof( int ) } );
            Assert.Equal( new[] { type.Methods[3], type.Methods[4] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", 0, new[] { typeof( object ), typeof( object ) } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods5 );
        }

        [Fact]
        public void Matches_IsStatic()
        {
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", isStatic: false );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", isStatic: true );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", isStatic: null );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Bar", isStatic: false );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Bar", isStatic: true );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Bar", isStatic: null );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods6 );
        }

        [Fact]
        public void Matches_Params()
        {
            var code = @"
class C
{
    public void Foo() // 0
    {
    }

    public void Foo(int x) // 1
    {
    }

    public void Foo(int x, int y) // 2
    {
    }

    public void Foo(object x) // 3
    {
    }

    public void Foo(object x, object y) // 4
    {
    }

    public void Foo( params int[] a) // 5
    {
    }

    public void Foo( params object[] a) // 6
    {
    }

    public void Foo( int x, params object[] a) // 7
    {
    }

    public void Foo( object x, params int[] a) // 8
    {
    }
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof( int ) );
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof( object ) );
            var intArrayType = compilation.Factory.GetTypeByReflectionType( typeof( int[] ) );
            var objectArrayType = compilation.Factory.GetTypeByReflectionType( typeof( object[] ) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", null, Array.Empty<IType>() );
            Assert.Equal( new[] { type.Methods[0], type.Methods[5], type.Methods[6] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { intType } );
            Assert.Equal( new[] { type.Methods[1], type.Methods[3], type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { intType, intType } );
            Assert.Equal( new[] { type.Methods[2], type.Methods[4], type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { intType, intType, intType } );
            Assert.Equal( new[] { type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { objectType } );
            Assert.Equal( new[] { type.Methods[3], type.Methods[6],  type.Methods[8] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { objectType, objectType } );
            Assert.Equal( new[] { type.Methods[4], type.Methods[6] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { objectType, objectType, objectType } );
            Assert.Equal( new[] { type.Methods[6] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { intArrayType } );
            Assert.Equal( new[] { type.Methods[3], type.Methods[5], type.Methods[6], type.Methods[8] }, matchedMethods8 );
            var matchedMethods9 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { objectArrayType } );
            Assert.Equal( new[] { type.Methods[3], type.Methods[6], type.Methods[8] }, matchedMethods9 );
            var matchedMethods10 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { intType, objectArrayType } );
            Assert.Equal( new[] { type.Methods[4], type.Methods[6], type.Methods[7] }, matchedMethods10 );
            var matchedMethods11 = type.Methods.OfCompatibleSignature( "Foo", null, new[] { objectType, intArrayType } );
            Assert.Equal( new[] { type.Methods[4], type.Methods[6], type.Methods[8] }, matchedMethods11 );
        }
    }
}