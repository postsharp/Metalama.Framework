using Caravela.Framework.Impl.Templating.Serialization;
using System;
using System.Collections.Generic;
using Xunit;
// ReSharper disable UnusedTypeParameter
// ReSharper disable ClassNeverInstantiated.Global

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class TypeNameUtilityTests
    {
        [Fact]
        public void TestTypeNameUtility()
        {
            var typesToTest = new Dictionary<string, Type>();
            typesToTest.Add( "System.String", typeof(string) );
            typesToTest.Add( "System.String[]", typeof(string[]) );
            typesToTest.Add( "System.Object[]", typeof(object[]) );
            typesToTest.Add( "System.Boolean[]", typeof(bool[]) );
            typesToTest.Add( "System.Object", typeof(object) );
            
            typesToTest.Add("System.Int32", typeof(int));
            typesToTest.Add("System.Double", typeof(double));
            typesToTest.Add("System.Single", typeof(float));
            typesToTest.Add("System.Boolean", typeof(bool));
            typesToTest.Add("System.Char", typeof(char));
            typesToTest.Add("System.Decimal", typeof(decimal));
            typesToTest.Add("System.Nullable<System.Decimal>[]", typeof(decimal?[]));
            typesToTest.Add("System.Nullable<System.Decimal>[][]", typeof(decimal?[][]));
            typesToTest.Add("System.Int64", typeof(long));
            typesToTest.Add("System.Guid", typeof(Guid));
            typesToTest.Add("System.Nullable<System.Int32>", typeof(int?));
            typesToTest.Add("System.Nullable<System.Double>", typeof(double?));
            typesToTest.Add("System.Nullable<System.Single>", typeof(float?));
            typesToTest.Add("System.Nullable<System.Boolean>", typeof(bool?));
            typesToTest.Add("System.Nullable<System.Char>",typeof(char?));
            typesToTest.Add("System.Nullable<System.Decimal>", typeof(decimal?));
            typesToTest.Add("System.Nullable<System.Int64>", typeof(long?));
            typesToTest.Add("System.Nullable<System.Guid>", typeof(Guid?));
            typesToTest.Add("System.Collections.Generic.List<System.String>", typeof(List<string>));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Guid>", typeof(Dictionary<string, Guid>));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Guid>[]", typeof(Dictionary<string, Guid>[]));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>", typeof(Dictionary<string, Guid?>));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>", typeof(Dictionary<string, Dictionary<string, Guid?>>));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[]", typeof(Dictionary<string, Dictionary<string, Guid?>>[]));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[][]", typeof(Dictionary<string, Dictionary<string, Guid?>>[][]));
            typesToTest.Add("System.Int32[]", typeof(int[]));
            typesToTest.Add("System.Int32[][]", typeof(int[][]));
            typesToTest.Add("System.Int32[][][]", typeof(int[][][]));
            typesToTest.Add("System.Int32[][][][]", typeof(int[][][][]));
            typesToTest.Add("System.Int32[][][][][]", typeof(int[][][][][]));
            typesToTest.Add("TestClass", typeof(TestClass));
            typesToTest.Add("System.Collections.Generic.List<TestClass>", typeof(List<TestClass>));
            typesToTest.Add("System.Collections.Generic.Dictionary<TestClass, TestClass>", typeof(Dictionary<TestClass, TestClass>));
            typesToTest.Add("System.Collections.Generic.Dictionary<System.String, TestClass>", typeof(Dictionary<string, TestClass>));
            typesToTest.Add("System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, TestClass>>", typeof(List<Dictionary<string, TestClass>>));
            typesToTest.Add("System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, GenericTestClass<System.String>>>", typeof(List<Dictionary<string, GenericTestClass<string>>>));
            typesToTest.Add("GenericTestClass<System.String>.SecondSubType<System.Decimal>", typeof(GenericTestClass<string>.SecondSubType<decimal>));
            typesToTest.Add("GenericTestClass<System.String>.SecondSubType", typeof(GenericTestClass<string>.SecondSubType));
            typesToTest.Add("GenericTestClass<System.String, System.Int32>.SecondSubType", typeof(GenericTestClass<string, int>.SecondSubType));
            typesToTest.Add("GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<System.String>", typeof(GenericTestClass<string, Dictionary<string,int>>.SecondSubType<string>));
            typesToTest.Add("GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>>", typeof(GenericTestClass<string, Dictionary<string, int>>.SecondSubType<GenericTestClass<string, Dictionary<string, int>>>));


            
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