// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class DeclarationComparerTests : UnitTestClass
    {
        [Fact]
        public void ConversionKindDefault()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A {}

interface I {}

class B : A, I
{
    public static implicit operator int(B a) => 42;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var typeA = compilation.Types.OfName( "A" ).Single();
            var typeB = compilation.Types.OfName( "B" ).Single();
            var typeI = compilation.Types.OfName( "I" ).Single();

            Assert.False( compilation.Comparers.Default.Is( typeA, typeof( int ), ConversionKind.Default ) );
            Assert.False( compilation.Comparers.Default.Is( typeA, typeof( bool ), ConversionKind.Default ) );
            Assert.False( compilation.Comparers.Default.Is( typeB, typeof( int ), ConversionKind.Default ) );

            Assert.False(
                compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof( int ) ), typeB, ConversionKind.Default ) );

            Assert.False( compilation.Comparers.Default.Is( typeA, typeB, ConversionKind.Default ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeA, ConversionKind.Default ) );
            Assert.False( compilation.Comparers.Default.Is( typeI, typeB, ConversionKind.Default ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeI, ConversionKind.Default ) );

            Assert.True(
                compilation.Comparers.Default.Is(
                    compilation.Factory.GetTypeByReflectionType( typeof( int ) ),
                    typeof( object ),
                    ConversionKind.Default ) );

            Assert.False(
                compilation.Comparers.Default.Is(
                    compilation.Factory.GetTypeByReflectionType( typeof( int ) ),
                    typeof( long ),
                    ConversionKind.Default ) );

            Assert.False(
                compilation.Comparers.Default.Is(
                    compilation.Factory.GetTypeByReflectionType( typeof( long ) ),
                    typeof( int ),
                    ConversionKind.Default ) );
        }

        [Fact]
        public void ConversionKindReference()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A {}

interface I {}

class B : A, I
{
    public static implicit operator int(B a) => 42;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var typeA = compilation.Types.OfName( "A" ).Single();
            var typeB = compilation.Types.OfName( "B" ).Single();
            var typeI = compilation.Types.OfName( "I" ).Single();

            Assert.False( compilation.Comparers.Default.Is( typeA, typeof( int ), ConversionKind.Reference ) );
            Assert.False( compilation.Comparers.Default.Is( typeA, typeof( bool ), ConversionKind.Reference ) );
            Assert.False( compilation.Comparers.Default.Is( typeB, typeof( int ), ConversionKind.Reference ) );
            Assert.False( compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof( int ) ), typeB, ConversionKind.Reference ) );
            Assert.False( compilation.Comparers.Default.Is( typeA, typeB, ConversionKind.Reference ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeA, ConversionKind.Reference ) );
            Assert.False( compilation.Comparers.Default.Is( typeI, typeB, ConversionKind.Reference ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeI, ConversionKind.Reference ) );

            Assert.False(
                compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof( int ) ), typeof( object ), ConversionKind.Reference ) );

            Assert.False(
                compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof( int ) ), typeof( long ), ConversionKind.Reference ) );

            Assert.False(
                compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof( long ) ), typeof( int ), ConversionKind.Reference ) );
        }

        [Fact]
        public void ConversionKindImplicit()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A {}

interface I {}

class B : A, I
{
    public static implicit operator int(B a) => 42;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var typeA = compilation.Types.OfName( "A" ).Single();
            var typeB = compilation.Types.OfName( "B" ).Single();
            var typeI = compilation.Types.OfName( "I" ).Single();

            Assert.False( compilation.Comparers.Default.Is( typeA, typeof(int), ConversionKind.Implicit ) );
            Assert.False( compilation.Comparers.Default.Is( typeA, typeof(bool), ConversionKind.Implicit ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeof(int), ConversionKind.Implicit ) );
            Assert.False( compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeB, ConversionKind.Implicit ) );
            Assert.False( compilation.Comparers.Default.Is( typeA, typeB, ConversionKind.Implicit ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeA, ConversionKind.Implicit ) );
            Assert.False( compilation.Comparers.Default.Is( typeI, typeB, ConversionKind.Implicit ) );
            Assert.True( compilation.Comparers.Default.Is( typeB, typeI, ConversionKind.Implicit ) );
            Assert.True( compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(object), ConversionKind.Implicit ) );
            Assert.True( compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(long), ConversionKind.Implicit ) );
            Assert.False( compilation.Comparers.Default.Is( compilation.Factory.GetTypeByReflectionType( typeof(long) ), typeof(int), ConversionKind.Implicit ) );
        }

        [Fact]
        public void ConversionKindTypeDefinition()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A<T> : I1 {}

interface I1 {}

interface I2<T> {}

class B<T> : I2<T> {}

class C<T> : A<T> {}

class D : B<int> {}

class E : C<int> {}
";

            var compilation = testContext.CreateCompilationModel( code );
            var typeA = compilation.Types.OfName( "A" ).Single();
            var typeB = compilation.Types.OfName( "B" ).Single();
            var typeC = compilation.Types.OfName( "C" ).Single();
            var typeD = compilation.Types.OfName( "D" ).Single();
            var typeE = compilation.Types.OfName( "E" ).Single();
            var typeI1 = compilation.Types.OfName( "I1" ).Single();
            var typeI2 = compilation.Types.OfName( "I2" ).Single();

            Assert.False( compilation.Comparers.Default.Is( typeD, typeA, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeD, typeB, ConversionKind.TypeDefinition ) );
            Assert.False( compilation.Comparers.Default.Is( typeD, typeC, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeD, typeD, ConversionKind.TypeDefinition ) );
            Assert.False( compilation.Comparers.Default.Is( typeD, typeI1, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeD, typeI2, ConversionKind.TypeDefinition ) );

            Assert.True( compilation.Comparers.Default.Is( typeE, typeA, ConversionKind.TypeDefinition ) );
            Assert.False( compilation.Comparers.Default.Is( typeE, typeB, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeE, typeC, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeE, typeE, ConversionKind.TypeDefinition ) );
            Assert.True( compilation.Comparers.Default.Is( typeE, typeI1, ConversionKind.TypeDefinition ) );
            Assert.False( compilation.Comparers.Default.Is( typeE, typeI2, ConversionKind.TypeDefinition ) );
        }
    }
}