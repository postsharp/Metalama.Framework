using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaLocalFunctionsTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _objectSerializers;

        // Local functions are not supported.
        // [Fact]
        public void TestLocalFunction()
        {
            string code = "class Target { void Method() {  int Local() { return 42; }  }  }";
            INamedType target = CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target");
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( target.Methods.GetValue().Single().LocalFunctions.Single() ) ).ToString();
            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Local", info.Name );
                Assert.Equal( typeof(int), info.ReturnParameter.ParameterType );
            } );
            this.AssertEqual( @"xxx", serialized );
        }

        public CaravelaLocalFunctionsTests(ITestOutputHelper helper) : base(helper) => this._objectSerializers = new ObjectSerializers();
    }
}