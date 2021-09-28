// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaEventInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestFieldLikeEvent()
        {
            var code = "class Target { public event System.Action Activated; }";
            var serialized = this.SerializeEvent( code );

            this.AssertEqual(
                @"typeof(global::Target).GetEvent(""Activated"")",
                serialized );

            TestExpression<EventInfo>(
                code,
                serialized,
                info =>
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
            var code = "class Target { public event System.Action Activated { add { } remove { } } }";
            var serialized = this.SerializeEvent( code );

            this.AssertEqual(
                @"typeof(global::Target).GetEvent(""Activated"")",
                serialized );

            TestExpression<EventInfo>(
                code,
                serialized,
                info =>
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
            var code = "class Target<TKey> { public event System.Func<TKey> Activated { add { } remove { } } }";
            var serialized = this.SerializeEvent( code );

            this.AssertEqual(
                @"typeof(global::Target<>).GetEvent(""Activated"")",
                serialized );

            TestExpression<EventInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.NotNull( info.AddMethod );
                    Assert.NotNull( info.RemoveMethod );
                    Assert.Equal( "Func`1", info.EventHandlerType!.Name );
                    Assert.Equal( "Target`1", info.DeclaringType!.Name );
                } );
        }

        private string SerializeEvent( string code )
        {
            var compilation = this.CreateCompilationModel( code );
            var single = compilation.Types.Single( t => t.Name == "Target" ).Events.Single( m => m.Name == "Activated" );
            var e = (single as Event)!;

            var actual = this
                .Serialize( new CompileTimeEventInfo( e ) )
                .ToString();

            return actual;
        }

        public CaravelaEventInfoTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}