using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaStateMachineTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _objectSerializers;
        public CaravelaStateMachineTests(ITestOutputHelper helper) : base(helper)
        {
            this._objectSerializers = new ObjectSerializers();
        }
        
        [Fact]
        public void TestEnumerable()
        {
            var code = "class Target { public static System.Collections.Generic.IEnumerable<int> Method() { yield return 2; } }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().First())).ToString();
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method~System.Collections.Generic.IEnumerable{System.Int32}""))", serialized );
            
            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Method", info.Name );
            } );
        }
        
        [Fact]
        public void TestAsync()
        {
            var code = "class Target { public static async void Method() { await System.Threading.Tasks.Task.Delay(1); } }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().First())).ToString();
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method""))", serialized );
            
            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Method", info.Name );
            } );
        }

      
    }
}