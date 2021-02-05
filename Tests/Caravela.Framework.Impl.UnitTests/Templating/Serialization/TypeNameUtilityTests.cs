using System;
using System.Collections.Generic;
using Caravela.Framework.Impl.Templating.Serialization;
using Xunit;

#pragma warning disable SA1402 // File may only contain a single type

// ReSharper disable UnusedTypeParameter
// ReSharper disable ClassNeverInstantiated.Global

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class TypeNameUtilityTests
    {
        [Fact]
        public void TestTypeNameUtility()
        {
            var typesToTest = new Dictionary<string, Type>
            {
                { "System.String", typeof( string ) },
                { "System.String[]", typeof( string[] ) },
                { "System.Object[]", typeof( object[] ) },
                { "System.Boolean[]", typeof( bool[] ) },
                { "System.Object", typeof( object ) },
                { "System.Int32", typeof( int ) },
                { "System.Double", typeof( double ) },
                { "System.Single", typeof( float ) },
                { "System.Boolean", typeof( bool ) },
                { "System.Char", typeof( char ) },
                { "System.Decimal", typeof( decimal ) },
                { "System.Nullable<System.Decimal>[]", typeof( decimal?[] ) },
                { "System.Nullable<System.Decimal>[][]", typeof( decimal?[][] ) },
                { "System.Int64", typeof( long ) },
                { "System.Guid", typeof( Guid ) },
                { "System.Nullable<System.Int32>", typeof( int? ) },
                { "System.Nullable<System.Double>", typeof( double? ) },
                { "System.Nullable<System.Single>", typeof( float? ) },
                { "System.Nullable<System.Boolean>", typeof( bool? ) },
                { "System.Nullable<System.Char>", typeof( char? ) },
                { "System.Nullable<System.Decimal>", typeof( decimal? ) },
                { "System.Nullable<System.Int64>", typeof( long? ) },
                { "System.Nullable<System.Guid>", typeof( Guid? ) },
                { "System.Collections.Generic.List<System.String>", typeof( List<string> ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Guid>", typeof( Dictionary<string, Guid> ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Guid>[]", typeof( Dictionary<string, Guid>[] ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>", typeof( Dictionary<string, Guid?> ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>", typeof( Dictionary<string, Dictionary<string, Guid?>> ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[]", typeof( Dictionary<string, Dictionary<string, Guid?>>[] ) },
                { "System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Guid>>>[][]", typeof( Dictionary<string, Dictionary<string, Guid?>>[][] ) },
                { "System.Int32[]", typeof( int[] ) },
                { "System.Int32[][]", typeof( int[][] ) },
                { "System.Int32[][][]", typeof( int[][][] ) },
                { "System.Int32[][][][]", typeof( int[][][][] ) },
                { "System.Int32[][][][][]", typeof( int[][][][][] ) },
                { "TestClass", typeof( TestClass ) },
                { "System.Collections.Generic.List<TestClass>", typeof( List<TestClass> ) },
                { "System.Collections.Generic.Dictionary<TestClass, TestClass>", typeof( Dictionary<TestClass, TestClass> ) },
                { "System.Collections.Generic.Dictionary<System.String, TestClass>", typeof( Dictionary<string, TestClass> ) },
                { "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, TestClass>>", typeof( List<Dictionary<string, TestClass>> ) },
                { "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, GenericTestClass<System.String>>>", typeof( List<Dictionary<string, GenericTestClass<string>>> ) },
                { "GenericTestClass<System.String>.SecondSubType<System.Decimal>", typeof( GenericTestClass<string>.SecondSubType<decimal> ) },
                { "GenericTestClass<System.String>.SecondSubType", typeof( GenericTestClass<string>.SecondSubType ) },
                { "GenericTestClass<System.String, System.Int32>.SecondSubType", typeof( GenericTestClass<string, int>.SecondSubType ) },
                { "GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<System.String>", typeof( GenericTestClass<string, Dictionary<string, int>>.SecondSubType<string> ) },
                { "GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>.SecondSubType<GenericTestClass<System.String, System.Collections.Generic.Dictionary<System.String, System.Int32>>>", typeof( GenericTestClass<string, Dictionary<string, int>>.SecondSubType<GenericTestClass<string, Dictionary<string, int>>> ) }
            };

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

    public class SecondSubType<T3>
    {
    }
}