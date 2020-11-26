using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaEventInfoTests : TestBase
    {
        [Fact]
        public void TestFieldLikeEvent()
        {
            string code = "class Target { public event System.Action Activated; }";
            string serialized = this.SerializeEvent( code );
            Assert.Equal( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Assert.NotNull( info.AddMethod );
                Assert.NotNull( info.RemoveMethod );
                Assert.Equal( typeof(Action), info.EventHandlerType );
                Assert.Equal( "Target", info.DeclaringType!.Name );
            } );
        }
        
        [Fact]
        public void TestCustomEvent()
        {
            string code = "class Target { public event System.Action Activated { add { } remove { } } }";
            string serialized = this.SerializeEvent( code );
            Assert.Equal( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Assert.NotNull( info.AddMethod );
                Assert.NotNull( info.RemoveMethod );
                Assert.Equal( typeof(Action), info.EventHandlerType );
                Assert.Equal( "Target", info.DeclaringType!.Name );
            } );
        }
        
        [Fact]
        public void TestCustomGenericEvent()
        {
            string code = "class Target<TKey> { public event System.Func<TKey> Activated { add { } remove { } } }";
            string serialized = this.SerializeEvent( code );
            Assert.Equal( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetEvent(""Activated"")", serialized );

            TestExpression<EventInfo>( code, serialized, ( info ) =>
            {
                Assert.NotNull( info.AddMethod );
                Assert.NotNull( info.RemoveMethod );
                Assert.Equal( "Func`1", info.EventHandlerType!.Name );
                Assert.Equal( "Target`1", info.DeclaringType!.Name );
            } );
        }

        private string SerializeEvent( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IEvent single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Events.GetValue().Single( m => m.Name == "Activated" );
            Event e = (single as Event)!;
            string actual = new CaravelaEventInfoSerializer(new CaravelaTypeSerializer()).Serialize( new CaravelaEventInfo( e.Symbol, (IType) e.ContainingElement! ) ).ToString();
            return actual;
        }
    }
}