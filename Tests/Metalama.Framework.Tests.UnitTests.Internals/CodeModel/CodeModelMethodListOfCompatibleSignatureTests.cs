// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Tests.UnitTests.Utilities;
using System;
using System.Linq;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelMethodListOfCompatibleSignatureTests : TestBase
    {
        [Fact]
        public void Matches_ParameterTypes()
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

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof(object) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: Array.Empty<IType>() );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[1], type.Methods[2] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType, intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[3], type.Methods[4] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { intType, objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Foo", argumentTypes: new[] { objectType, objectType, objectType } ).ToArray();
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods8 );
        }

        [Fact]
        public void Matches_ParameterTypeRefKinds()
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

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType } );
            Assert.Equal( new[] { type.Methods[0], type.Methods[1] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[0], type.Methods[1] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType }, new RefKind?[] { RefKind.Ref } );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType }, new RefKind?[] { RefKind.RefReadOnly } );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Bar", new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[2], type.Methods[3] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Bar", new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Bar", new[] { intType }, new RefKind?[] { RefKind.Out } );
            Assert.Equal( new[] { type.Methods[3] }, matchedMethods8 );
            var matchedMethods9 = type.Methods.OfCompatibleSignature( "Quz", new[] { intType }, new RefKind?[] { null } );
            Assert.Equal( new[] { type.Methods[4], type.Methods[5] }, matchedMethods9 );
            var matchedMethods10 = type.Methods.OfCompatibleSignature( "Quz", new[] { intType }, new RefKind?[] { RefKind.None } );
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods10 );
            var matchedMethods11 = type.Methods.OfCompatibleSignature( "Quz", new[] { intType }, new RefKind?[] { RefKind.In } );
            Assert.Equal( new[] { type.Methods[5] }, matchedMethods11 );
        }

        [Fact]
        public void Matches_ParameterReflectionTypes()
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

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>() ).ToArray();
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", new[] { typeof(int) } ).ToArray();
            Assert.Equal( new[] { type.Methods[1], type.Methods[2] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", new[] { typeof(object) } ).ToArray();
            Assert.Equal( new[] { type.Methods[2] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", new[] { typeof(int), typeof(int) } ).ToArray();
            Assert.Equal( new[] { type.Methods[3], type.Methods[4] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", new[] { typeof(object), typeof(object) } ).ToArray();
            Assert.Equal( new[] { type.Methods[4] }, matchedMethods5 );
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

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>(), isStatic: false );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>(), isStatic: true );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>(), isStatic: null );
            Assert.Equal( new[] { type.Methods[0] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Bar", Array.Empty<IType>(), isStatic: false );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Bar", Array.Empty<IType>(), isStatic: true );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Bar", Array.Empty<IType>(), isStatic: null );
            Assert.Equal( new[] { type.Methods[1] }, matchedMethods6 );
        }

        [Fact]
        public void Matches_Params()
        {
            using var testContext = this.CreateTestContext();

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

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types[0];
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );
            var objectType = compilation.Factory.GetTypeByReflectionType( typeof(object) );
            var intArrayType = compilation.Factory.GetTypeByReflectionType( typeof(int[]) );
            var objectArrayType = compilation.Factory.GetTypeByReflectionType( typeof(object[]) );

            var matchedMethods1 = type.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>() );
            Assert.Equal( new[] { type.Methods[0], type.Methods[5], type.Methods[6] }, matchedMethods1 );
            var matchedMethods2 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[1], type.Methods[3], type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods2 );
            var matchedMethods3 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType, intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[2], type.Methods[4], type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods3 );
            var matchedMethods4 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType, intType, intType } ).ToArray();
            Assert.Equal( new[] { type.Methods[5], type.Methods[6], type.Methods[7], type.Methods[8] }, matchedMethods4 );
            var matchedMethods5 = type.Methods.OfCompatibleSignature( "Foo", new[] { objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[3], type.Methods[6], type.Methods[8] }, matchedMethods5 );
            var matchedMethods6 = type.Methods.OfCompatibleSignature( "Foo", new[] { objectType, objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4], type.Methods[6] }, matchedMethods6 );
            var matchedMethods7 = type.Methods.OfCompatibleSignature( "Foo", new[] { objectType, objectType, objectType } ).ToArray();
            Assert.Equal( new[] { type.Methods[6] }, matchedMethods7 );
            var matchedMethods8 = type.Methods.OfCompatibleSignature( "Foo", new[] { intArrayType } ).ToArray();
            Assert.Equal( new[] { type.Methods[3], type.Methods[5], type.Methods[6], type.Methods[8] }, matchedMethods8 );
            var matchedMethods9 = type.Methods.OfCompatibleSignature( "Foo", new[] { objectArrayType } ).ToArray();
            Assert.Equal( new[] { type.Methods[3], type.Methods[6], type.Methods[8] }, matchedMethods9 );
            var matchedMethods10 = type.Methods.OfCompatibleSignature( "Foo", new[] { intType, objectArrayType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4], type.Methods[6], type.Methods[7] }, matchedMethods10 );
            var matchedMethods11 = type.Methods.OfCompatibleSignature( "Foo", new[] { objectType, intArrayType } ).ToArray();
            Assert.Equal( new[] { type.Methods[4], type.Methods[6], type.Methods[8] }, matchedMethods11 );
        }

        [Fact]
        public void Matches_InheritanceHierarchy()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class A
{
    public void Foo() {}
    public void Bar() {}
    public virtual void Baz() {}
    public virtual void Qux() {}
}

class B : A
{
    public new void Foo() {}
    public override void Baz() {}
    public void Quz() {}
    public virtual void Qur() {}
    public void Trx() {}
    public virtual void Trw() {}
}

class C : B
{
    public new void Trx() {}
    public override void Trw() {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var types = compilation.Types.OrderBySource();
            var typeA = types[0];
            var typeB = types[1];
            var typeC = types[2];

            var matchedMethods1 = typeC.Methods.OfCompatibleSignature( "Foo", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeB.Methods[0] }, matchedMethods1 );
            var matchedMethods2 = typeC.Methods.OfCompatibleSignature( "Bar", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeA.Methods[1] }, matchedMethods2 );
            var matchedMethods3 = typeC.Methods.OfCompatibleSignature( "Baz", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeB.Methods[1] }, matchedMethods3 );
            var matchedMethods4 = typeC.Methods.OfCompatibleSignature( "Qux", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeA.Methods[3] }, matchedMethods4 );
            var matchedMethods5 = typeC.Methods.OfCompatibleSignature( "Quz", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeB.Methods[2] }, matchedMethods5 );
            var matchedMethods6 = typeC.Methods.OfCompatibleSignature( "Qur", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeB.Methods[3] }, matchedMethods6 );
            var matchedMethods7 = typeC.Methods.OfCompatibleSignature( "Trx", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeC.Methods[0] }, matchedMethods7 );
            var matchedMethods8 = typeC.Methods.OfCompatibleSignature( "Trw", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( new[] { typeC.Methods[1] }, matchedMethods8 );
            var matchedMethods9 = typeC.Methods.OfCompatibleSignature( "Xyzzy", Array.Empty<IType>(), declaredOnly: false );
            Assert.Equal( Array.Empty<IMethod>(), matchedMethods9 );
        }
    }
}