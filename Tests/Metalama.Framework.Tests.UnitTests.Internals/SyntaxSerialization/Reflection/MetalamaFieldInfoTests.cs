// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection
{
    public class MetalamaFieldInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestField()
        {
            var code = "class Target { public int Field; }";
            var serialized = this.SerializeField( code );

            // TODO: This should emit a call to Intrinsics.

            this.AssertEqual(
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target).GetField(""Field"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<FieldInfo>(
                code,
                MetalamaPropertyInfoTests.StripLocationInfo( serialized ),
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

            // TODO: This should emit a call to Intrinsics.

            this.AssertEqual(
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target<>).GetField(""Field"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<FieldInfo>(
                code,
                MetalamaPropertyInfoTests.StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Field", info.Name );
                    Assert.Equal( "TKey[]", info.FieldType.Name );
                } );
        }

        private string SerializeField( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Fields.Single( m => m.Name == "Field" );
            var actual = testContext.Serialize( CompileTimeFieldOrPropertyInfo.Create( (Field) single ) ).ToString();

            return actual;
        }

        public MetalamaFieldInfoTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}