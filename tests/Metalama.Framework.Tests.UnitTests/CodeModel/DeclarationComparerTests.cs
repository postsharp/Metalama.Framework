// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Comparers;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public sealed class DeclarationComparerTests : UnitTestClass
    {
        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void ConversionKindDefault( bool bypassSymbols )
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

            var comparer = (DeclarationEqualityComparer) compilation.CompilationContext.Comparers.Default;

            // ReSharper disable RedundantArgumentDefaultValue
            Assert.False( comparer.Is( typeA, typeof(int), ConversionKind.Default, bypassSymbols ) );
            Assert.False( comparer.Is( typeA, typeof(bool), ConversionKind.Default, bypassSymbols ) );
            Assert.False( comparer.Is( typeB, typeof(int), ConversionKind.Default, bypassSymbols ) );

            Assert.False( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeB, ConversionKind.Default, bypassSymbols ) );

            Assert.False( comparer.Is( typeA, typeB, ConversionKind.Default, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeA, ConversionKind.Default, bypassSymbols ) );
            Assert.False( comparer.Is( typeI, typeB, ConversionKind.Default, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeI, ConversionKind.Default, bypassSymbols ) );

            Assert.True(
                comparer.Is(
                    compilation.Factory.GetTypeByReflectionType( typeof(int) ),
                    typeof(object),
                    ConversionKind.Default,
                    bypassSymbols ) );

            if ( !bypassSymbols )
            {
                // Built-in implicit numeric conversions are not supported in bypassSymbols mode.

                Assert.False(
                    comparer.Is(
                        compilation.Factory.GetTypeByReflectionType( typeof(int) ),
                        typeof(long),
                        ConversionKind.Default ) );

                Assert.False(
                    comparer.Is(
                        compilation.Factory.GetTypeByReflectionType( typeof(long) ),
                        typeof(int),
                        ConversionKind.Default ) );
            }

            // ReSharper restore RedundantArgumentDefaultValue
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void ConversionKindReference( bool bypassSymbols )
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

            var comparer = (DeclarationEqualityComparer) compilation.CompilationContext.Comparers.Default;

            Assert.False( comparer.Is( typeA, typeof(int), ConversionKind.Reference, bypassSymbols ) );
            Assert.False( comparer.Is( typeA, typeof(bool), ConversionKind.Reference, bypassSymbols ) );
            Assert.False( comparer.Is( typeB, typeof(int), ConversionKind.Reference, bypassSymbols ) );
            Assert.False( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeB, ConversionKind.Reference, bypassSymbols ) );
            Assert.False( comparer.Is( typeA, typeB, ConversionKind.Reference, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeA, ConversionKind.Reference, bypassSymbols ) );
            Assert.False( comparer.Is( typeI, typeB, ConversionKind.Reference, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeI, ConversionKind.Reference, bypassSymbols ) );

            Assert.False(
                comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(object), ConversionKind.Reference, bypassSymbols ) );

            Assert.False(
                comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(long), ConversionKind.Reference, bypassSymbols ) );

            Assert.False(
                comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(long) ), typeof(int), ConversionKind.Reference, bypassSymbols ) );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void ConversionKindImplicit( bool bypassSymbols )
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

            var comparer = (DeclarationEqualityComparer) compilation.CompilationContext.Comparers.Default;

            Assert.False( comparer.Is( typeA, typeof(int), ConversionKind.Implicit, bypassSymbols ) );
            Assert.False( comparer.Is( typeA, typeof(bool), ConversionKind.Implicit, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeof(int), ConversionKind.Implicit, bypassSymbols ) );
            Assert.False( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeB, ConversionKind.Implicit, bypassSymbols ) );
            Assert.False( comparer.Is( typeA, typeB, ConversionKind.Implicit, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeA, ConversionKind.Implicit, bypassSymbols ) );
            Assert.False( comparer.Is( typeI, typeB, ConversionKind.Implicit, bypassSymbols ) );
            Assert.True( comparer.Is( typeB, typeI, ConversionKind.Implicit, bypassSymbols ) );

            Assert.True( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(object), ConversionKind.Implicit, bypassSymbols ) );

            if ( !bypassSymbols )
            {
                // Built-in implicit numeric conversions are not supported in bypassSymbols mode.

                Assert.True( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(int) ), typeof(long), ConversionKind.Implicit, bypassSymbols ) );

                Assert.False( comparer.Is( compilation.Factory.GetTypeByReflectionType( typeof(long) ), typeof(int), ConversionKind.Implicit, bypassSymbols ) );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void ConversionKindTypeDefinition( bool bypassSymbols )
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

            var comparer = (DeclarationEqualityComparer) compilation.CompilationContext.Comparers.Default;

            Assert.False( comparer.Is( typeD, typeA, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeD, typeB, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.False( comparer.Is( typeD, typeC, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeD, typeD, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.False( comparer.Is( typeD, typeI1, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeD, typeI2, ConversionKind.TypeDefinition, bypassSymbols ) );

            Assert.True( comparer.Is( typeE, typeA, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.False( comparer.Is( typeE, typeB, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeE, typeC, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeE, typeE, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.True( comparer.Is( typeE, typeI1, ConversionKind.TypeDefinition, bypassSymbols ) );
            Assert.False( comparer.Is( typeE, typeI2, ConversionKind.TypeDefinition, bypassSymbols ) );
        }
    }
}