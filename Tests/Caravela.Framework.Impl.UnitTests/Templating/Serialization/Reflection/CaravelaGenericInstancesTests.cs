using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaGenericInstancesTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _objectSerializers;

        public CaravelaGenericInstancesTests( ITestOutputHelper helper ) : base(helper) => this._objectSerializers = new ObjectSerializers();

        [Fact]
        public void TestListString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.List<string> Target; } }", 
                typeof(List<string>), 
                @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Collections.Generic.List`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")))");
        }
        
        [Fact]
        public void TestDictionaryString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.Dictionary<string[],int?> Target; } }", 
                typeof(Dictionary<string[],int?>), 
                @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Collections.Generic.Dictionary`2"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")).MakeArrayType(), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Nullable`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))))");
        }
        
        private void AssertFieldType(string code, Type expectedType, string expected)
        {
            var allTypes = CreateCompilation(code).DeclaredTypes.GetValue();
            var nestedTypes = allTypes.Single().NestedTypes.GetValue();
            var innerType = nestedTypes.Single();
            var allProperties = innerType.Properties.GetValue();
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression(
                    CaravelaType.Create(
                        allProperties.Single().Type))
                .ToString();

            TestExpression<Type>(code, serialized, (info) => { Assert.Equal(expectedType, info); });
            Assert.Equal(expected, serialized);
        }
    }
}