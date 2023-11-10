// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.


#define ROSLYN_4_8_0_OR_GREATER

using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public sealed class ConstructorIsPrimaryTests : UnitTestClass
    {
        [Fact]
        public void ImplicitConstructor_Class()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeClass = compilation.Types.OfName( "A" ).Single();

            Assert.Null( typeClass.PrimaryConstructor );
            Assert.All( typeClass.Constructors, c => Assert.False( c.IsPrimary ) );
        }
        [Fact]
        public void ImplicitConstructor_Struct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
struct B {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeStruct = compilation.Types.OfName( "B" ).Single();

            Assert.Null( typeStruct.PrimaryConstructor );
            Assert.All( typeStruct.Constructors, c => Assert.False( c.IsPrimary ) );
        }
        [Fact]
        public void ImplicitConstructor_RecordClass()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record class C {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordClass = compilation.Types.OfName( "C" ).Single();

            Assert.Null( typeRecordClass.PrimaryConstructor );
            Assert.All( typeRecordClass.Constructors, c => Assert.False( c.IsPrimary ) );
        }
        [Fact]
        public void ImplicitConstructor_RecordStruct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record struct D {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordStruct = compilation.Types.OfName( "D" ).Single();

            Assert.Null( typeRecordStruct.PrimaryConstructor );
            Assert.All( typeRecordStruct.Constructors, c => Assert.False( c.IsPrimary ) );
        }
        [Fact]
        public void ImplicitConstructor_Enum()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
enum E {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeEnum = compilation.Types.OfName( "E" ).Single();

            Assert.Null( typeEnum.PrimaryConstructor );
            Assert.All( typeEnum.Constructors, c => Assert.False( c.IsPrimary ) );
        }
        [Fact]
        public void ImplicitConstructor_Delegate()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
delegate void F();
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeDelegate = compilation.Types.OfName( "F" ).Single();

            Assert.Null( typeDelegate.PrimaryConstructor );
            Assert.All( typeDelegate.Constructors, c => Assert.False( c.IsPrimary ) );
        }

        [Fact]
        public void ExplicitConstructor_Class()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A { public A() {} public A(int x) {} }
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeClass = compilation.Types.OfName( "A" ).Single();

            Assert.Null( typeClass.PrimaryConstructor );
            Assert.All( typeClass.Constructors, c => Assert.False( c.IsPrimary ) );
        }

        [Fact]
        public void ExplicitConstructor_Struct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
struct B { public B() {} public B(int x) {} }
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeStruct = compilation.Types.OfName( "B" ).Single();

            Assert.Null( typeStruct.PrimaryConstructor );
            Assert.All( typeStruct.Constructors, c => Assert.False( c.IsPrimary ) );
        }

        [Fact]
        public void ExplicitConstructor_RecordClass()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record class C { public C() {} public C(int x) {} }
record struct D { public D() {} public D(int x) {} }
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordClass = compilation.Types.OfName( "C" ).Single();

            Assert.Null( typeRecordClass.PrimaryConstructor );
            Assert.All( typeRecordClass.Constructors, c => Assert.False( c.IsPrimary ) );
        }

        [Fact]
        public void ExplicitConstructor_RecordStruct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record struct D { public D() {} public D(int x) {} }
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordStruct = compilation.Types.OfName( "D" ).Single();

            Assert.Null( typeRecordStruct.PrimaryConstructor );
            Assert.All( typeRecordStruct.Constructors, c => Assert.False( c.IsPrimary ) );
        }

#if ROSLYN_4_8_0_OR_GREATER
        [Fact]
        public void ParameterlessPrimaryConstructor_Class()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A() {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeClass = compilation.Types.OfName( "A" ).Single();

            Assert.NotNull( typeClass.PrimaryConstructor );
            Assert.Single( typeClass.Constructors );
            Assert.Equal( typeClass.Constructors.Single(), typeClass.PrimaryConstructor );
        }

        [Fact]
        public void ParameterlessPrimaryConstructor_Struct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
struct B() {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeStruct = compilation.Types.OfName( "B" ).Single();

            Assert.NotNull( typeStruct.PrimaryConstructor );
            Assert.Single( typeStruct.Constructors );
            Assert.Equal( typeStruct.Constructors.Single(), typeStruct.PrimaryConstructor );
        }
#endif

        [Fact]
        public void ParameterlessPrimaryConstructor_RecordClass()
        {
            using var testContext = this.CreateTestContext();

            const string code =
                @"
record class C() {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordClass = compilation.Types.OfName( "C" ).Single();

            Assert.NotNull( typeRecordClass.PrimaryConstructor );
            Assert.Single( typeRecordClass.Constructors );
            Assert.Equal( typeRecordClass.Constructors.Single(), typeRecordClass.PrimaryConstructor );
        }

        [Fact]
        public void ParameterlessPrimaryConstructor_RecordStruct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record struct D() {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordStruct = compilation.Types.OfName( "D" ).Single();

            Assert.NotNull( typeRecordStruct.PrimaryConstructor );
            Assert.Single( typeRecordStruct.Constructors );
            Assert.Equal( typeRecordStruct.Constructors.Single(), typeRecordStruct.PrimaryConstructor );
        }

        [Fact]
        public void PrimaryConstructor_Class()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class A(int x) {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeClass = compilation.Types.OfName( "A" ).Single();

            Assert.NotNull( typeClass.PrimaryConstructor );
            Assert.Single( typeClass.Constructors );
            Assert.Equal( typeClass.Constructors.Single(), typeClass.PrimaryConstructor );
        }

        [Fact]
        public void PrimaryConstructor_Struct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
struct B(int x) {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeStruct = compilation.Types.OfName( "B" ).Single();

            Assert.NotNull( typeStruct.PrimaryConstructor );
            Assert.Single( typeStruct.Constructors.Where( c => c.Parameters.Count == 1 ) );
            Assert.Single( typeStruct.Constructors.Where( c => c.Parameters.Count != 1 ) );
            Assert.Equal( typeStruct.Constructors.Where( c => c.Parameters.Count == 1 ).Single(), typeStruct.PrimaryConstructor );
        }

        [Fact]
        public void PrimaryConstructor_RecordClass()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record class C(int x) {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordClass = compilation.Types.OfName( "C" ).Single();

            Assert.NotNull( typeRecordClass.PrimaryConstructor );
            Assert.Single( typeRecordClass.Constructors );
            Assert.Equal( typeRecordClass.Constructors.Single(), typeRecordClass.PrimaryConstructor );
        }

        [Fact]
        public void PrimaryConstructor_RecordStruct()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
record struct D(int x) {}
";

            var compilation = testContext.CreateCompilationModel( code );

            var typeRecordStruct = compilation.Types.OfName( "D" ).Single();

            Assert.NotNull( typeRecordStruct.PrimaryConstructor );
            Assert.Single( typeRecordStruct.Constructors.Where( c => c.Parameters.Count == 1 ) );
            Assert.Single( typeRecordStruct.Constructors.Where( c => c.Parameters.Count != 1 ) );
            Assert.Equal( typeRecordStruct.Constructors.Where( c => c.Parameters.Count == 1 ).Single(), typeRecordStruct.PrimaryConstructor );
        }
    }
}
