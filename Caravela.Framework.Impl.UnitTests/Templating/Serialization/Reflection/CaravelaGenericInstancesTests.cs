using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaGenericInstancesTests : TestBase
    {
        private readonly ObjectSerializers _objectSerializers;

        public CaravelaGenericInstancesTests() => this._objectSerializers = new ObjectSerializers();

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
            INamedType innerType = CreateCompilation(code).DeclaredTypes.GetValue().Single().NestedTypes.GetValue().Single();
            IEnumerable<IProperty> allPropreties = innerType.AllProperties.GetValue();
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression(
                    CaravelaType.Create(
                        allPropreties.Single().Type))
                .ToString();
            

            TestExpression<Type>(code, serialized, (info) => { Assert.Equal(expectedType, info); });

            Assert.Equal(expected, serialized);
        }
    }
}