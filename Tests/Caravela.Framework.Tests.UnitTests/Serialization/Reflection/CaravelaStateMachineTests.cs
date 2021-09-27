// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaStateMachineTests : ReflectionTestBase
    {
        public CaravelaStateMachineTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void TestEnumerable()
        {
            var code = "class Target { public static System.Collections.Generic.IEnumerable<int> Method() { yield return 2; } }";

            var serialized = this.Serialize(
                    CompileTimeMethodInfo.Create( CreateCompilationModel( code ).Types.Single( t => t.Name == "Target" ).Methods.First() ) )
                .ToString();

            this.AssertEqual(
                @"((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method~System.Collections.Generic.IEnumerable{System.Int32}"")))",
                serialized );

            TestExpression<MethodInfo>( code, serialized, info => Assert.Equal( "Method", info.Name ) );
        }

        [Fact]
        public void TestAsync()
        {
            var code = "class Target { public static async void Method() { await System.Threading.Tasks.Task.Delay(1); } }";

            var serialized = this.Serialize(
                    CompileTimeMethodInfo.Create( CreateCompilationModel( code ).Types.Single( t => t.Name == "Target" ).Methods.First() ) )
                .ToString();

            this.AssertEqual(
                @"((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method"")))",
                serialized );

            TestExpression<MethodInfo>( code, serialized, info => Assert.Equal( "Method", info.Name ) );
        }
    }
}