using Caravela.Framework.Code;
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
            string code = "class Target { public static System.Collections.Generic.IEnumerable<int> Method() { yield return 2; } }";
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().Single())).ToString();
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method~System.Collections.Generic.IEnumerable{System.Int32}""))", serialized );
            
            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Method", info.Name );
            } );
        }
        
        [Fact]
        public void TestAsync()
        {
            string code = "class Target { public static async void Method() { await System.Threading.Tasks.Task.Delay(1); } }";
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().Single())).ToString();
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method""))", serialized );
            
            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Method", info.Name );
            } );
        }

      
    }
}