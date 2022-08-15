// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection
{
    public class MetalamaEventInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestFieldLikeEvent()
        {
            var code = "class Target { public event System.Action Activated; }";
            var serialized = this.SerializeEvent( code );

            this.AssertEqual(
                @"typeof(global::Target).GetEvent(""Activated"")",
                serialized );

            this.TestExpression<EventInfo>(
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

            this.TestExpression<EventInfo>(
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

            this.TestExpression<EventInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.NotNull( info.AddMethod );
                    Assert.NotNull( info.RemoveMethod );
                    Assert.Equal( "Func`1", info.EventHandlerType.AssertNotNull().Name );
                    Assert.Equal( "Target`1", info.DeclaringType!.Name );
                } );
        }

        private string SerializeEvent( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Events.Single( m => m.Name == "Activated" );
            var e = (single as Event)!;

            var actual = testContext
                .Serialize( new CompileTimeEventInfo( e ) )
                .ToString();

            return actual;
        }

        public MetalamaEventInfoTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}