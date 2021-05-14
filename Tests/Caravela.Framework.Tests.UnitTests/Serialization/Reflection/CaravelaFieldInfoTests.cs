// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaFieldInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestField()
        {
            var code = "class Target { public int Field; }";
            var serialized = this.SerializeField( code );

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetField(""Field"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<FieldInfo>(
                code,
                CaravelaPropertyInfoTests.StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Field", info.Name );
                    Assert.Equal( typeof(int), info.FieldType );
                } );
        }

        [Fact]
        public void TestFieldGeneric()
        {
            var code = "class Target<TKey> { public TKey[] Field; }";
            var serialized = this.SerializeField( code );

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetField(""Field"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<FieldInfo>(
                code,
                CaravelaPropertyInfoTests.StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Field", info.Name );
                    Assert.Equal( "TKey[]", info.FieldType.Name );
                } );
        }

        private string SerializeField( string code )
        {
            var compilation = CreateCompilationModel( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Fields.Single( m => m.Name == "Field" );
            var actual = this.Serialize( CompileTimeFieldOrPropertyInfo.Create( (Field) single ) ).ToString();

            return actual;
        }

        public CaravelaFieldInfoTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}