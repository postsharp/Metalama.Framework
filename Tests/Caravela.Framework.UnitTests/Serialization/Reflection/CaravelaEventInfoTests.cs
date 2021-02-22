using System;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaEventInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestFieldLikeEvent()
        {
            var code = "class Target { public event System.Action Activated; }";
            var serialized = this.SerializeEvent( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Xunit.Assert.NotNull( info.AddMethod );
                Xunit.Assert.NotNull( info.RemoveMethod );
                Xunit.Assert.Equal<Type>( typeof( Action ), info.EventHandlerType );
                Xunit.Assert.Equal( "Target", info.DeclaringType!.Name );
            } );
        }

        [Fact]
        public void TestCustomEvent()
        {
            var code = "class Target { public event System.Action Activated { add { } remove { } } }";
            var serialized = this.SerializeEvent( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Xunit.Assert.NotNull( info.AddMethod );
                Xunit.Assert.NotNull( info.RemoveMethod );
                Xunit.Assert.Equal<Type>( typeof( Action ), info.EventHandlerType );
                Xunit.Assert.Equal( "Target", info.DeclaringType!.Name );
            } );
        }

        [Fact]
        public void TestCustomGenericEvent()
        {
            var code = "class Target<TKey> { public event System.Func<TKey> Activated { add { } remove { } } }";
            var serialized = this.SerializeEvent( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Xunit.Assert.NotNull( info.AddMethod );
                Xunit.Assert.NotNull( info.RemoveMethod );
                Xunit.Assert.Equal( "Func`1", info.EventHandlerType!.Name );
                Xunit.Assert.Equal( "Target`1", info.DeclaringType!.Name );
            } );
        }

        private string SerializeEvent( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Events.Single( m => m.Name == "Activated" );
            var e = (single as Event)!;
            var actual = new CaravelaEventInfoSerializer( new CaravelaTypeSerializer() ).Serialize( new CaravelaEventInfo( e.Symbol, (IType) e.ContainingElement! ) ).ToString();
            return actual;
        }

        public CaravelaEventInfoTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}