using Caravela.Framework.Impl.Templating.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class TypeNameUtilityTests
    {
        [Fact]
        public void TestTypeNameUtility()
        {
            Dictionary<string, Type> typesToTest = new Dictionary<string, Type>();
            typesToTest.Add( "System.String", typeof(string) );
            typesToTest.Add( "System.String[]", typeof(string[]) );
            typesToTest.Add( "System.Object[]", typeof(object[]) );
            typesToTest.Add( "System.Boolean[]", typeof(bool[]) );
            typesToTest.Add( "System.Object", typeof(object) );
            // typesToTest.Add(typeof(int));
            // typesToTest.Add(typeof(double));
            // typesToTest.Add(typeof(float));
            // typesToTest.Add(typeof(bool));
            // typesToTest.Add(typeof(char));
            // typesToTest.Add(typeof(decimal));
            // typesToTest.Add(typeof(decimal?[]));
            // typesToTest.Add(typeof(decimal?[][]));
            // typesToTest.Add(typeof(Int64));
            // typesToTest.Add(typeof(Guid));
            // typesToTest.Add(typeof(int?));
            // typesToTest.Add(typeof(double?));
            typesToTest.Add("System.Nullable<System.Single>", typeof(float?));
            typesToTest.Add("System.Nullable<System.Boolean>", typeof(bool?));
            typesToTest.Add("System.Nullable<System.Char>",typeof(char?));
            // typesToTest.Add(typeof(decimal?));
            // typesToTest.Add(typeof(Int64?));
            // typesToTest.Add(typeof(Guid?));
            // typesToTest.Add(typeof(List<string>));
            // typesToTest.Add(typeof(Dictionary<string, Guid>));
            // typesToTest.Add(typeof(Dictionary<string, Guid>[]));
            // typesToTest.Add(typeof(Dictionary<string, Guid?>));
            // typesToTest.Add(typeof(Dictionary<string, Dictionary<string, Guid?>>));
            // typesToTest.Add(typeof(Dictionary<string, Dictionary<string, Guid?>>[]));
            // typesToTest.Add(typeof(Dictionary<string, Dictionary<string, Guid?>>[][]));
            // typesToTest.Add(typeof(int[]));
            // typesToTest.Add(typeof(int[][]));
            // typesToTest.Add(typeof(int[][][]));
            // typesToTest.Add(typeof(int[][][][]));
            // typesToTest.Add(typeof(int[][][][][]));
            // typesToTest.Add(typeof(TestClass));
            // typesToTest.Add(typeof(List<TestClass>));
            // typesToTest.Add(typeof(Dictionary<TestClass, TestClass>));
            // typesToTest.Add(typeof(Dictionary<string, TestClass>));
            // typesToTest.Add(typeof(List<Dictionary<string, TestClass>>));
            // typesToTest.Add(typeof(List<Dictionary<string, GenericTestClass<string>>>));
            // typesToTest.Add(typeof(GenericTestClass<string>.SecondSubType<decimal>));
            // typesToTest.Add(typeof(GenericTestClass<string>.SecondSubType));
            // typesToTest.Add(typeof(GenericTestClass<string, int>.SecondSubType));
            // typesToTest.Add(typeof(GenericTestClass<string, Dictionary<string,int>>.SecondSubType<string>));
            // typesToTest.Add(typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<GenericTestClass<string, Dictionary<string, int>>>));


            foreach ( var t in typesToTest )
            {
                var actualName = TypeNameUtility.ToCSharpQualifiedName( t.Value );
                Assert.Equal( t.Key, actualName );
            }
        }
    }
}

public class TestClass
{
}

public class GenericTestClass<T>
{
    public class SecondSubType
    {
    }

    public class SecondSubType<T2>
    {
    }
}

public class GenericTestClass<T1, T2>
{
    public class SecondSubType
    {
    }

    public class SecondSubType<T2>
    {
    }
}