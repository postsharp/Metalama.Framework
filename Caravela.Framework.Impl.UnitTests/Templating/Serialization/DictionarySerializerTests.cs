using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class DictionarySerializerTests
    {
        private readonly ObjectSerializers _serializers;
        private readonly DictionarySerializer _serializer;

        public DictionarySerializerTests()
        {
            this._serializers = new ObjectSerializers();
            this._serializer = new DictionarySerializer( this._serializers );
        }
        
        [Fact]
        public void TestEmptyDictionary()
        {
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.Int32, System.Int32>{}", new Dictionary<int, int>() );
        }
        
        [Fact]
        public void TestBasicDictionary()
        {
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.Int32, System.Int32>{{4, 8}}", new Dictionary<int, int>() {{4, 8}} );
        }
        
        [Fact]
        public void TestNestedDictionary()
        {
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.Int32, System.Nullable<System.Int32>>{{4, 8}, {6, null}}", new Dictionary<int, int?>() {{4, 8}, {6, null}} );
        }
        [Fact]
        public void TestRecursiveDictionary_InKey()
        {
            var d = new Dictionary<object, object>();
            d.Add( d, "20" );
            Assert.Throws<CaravelaException>( () =>
            {
                this._serializer.SerializeObject( d );
            } );
        }
        [Fact]
        public void TestRecursiveDictionary_InValue()
        {
            var d = new Dictionary<object, object>();
            d.Add( "20", d );
            Assert.Throws<CaravelaException>( () =>
            {
                this._serializer.SerializeObject( d );
            } );
        }
        
        [Fact]
        public void TestStringDictionary()
        {
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Nullable<System.Int32>>{{\"A\", 8}, {\"B\", null}}", new Dictionary<string, int?>() {{"A", 8}, {"B", null}} );
        }
        
        [Fact]
        public void TestStringDictionaryWithComparer()
        {
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.Ordinal)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.Ordinal) {{"A", 8}} );
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.OrdinalIgnoreCase)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {{"A", 8}} );
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.InvariantCulture)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.InvariantCulture) {{"A", 8}} );
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.InvariantCultureIgnoreCase)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase) {{"A", 8}} );
            
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.CurrentCulture)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.CurrentCulture) {{"A", 8}} );
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>(System.StringComparer.CurrentCultureIgnoreCase)\r\n{{\"A\", 8}}",
                new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase) {{"A", 8}} );
            
            
            // default comparer:
            this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Int32>{{\"A\", 8}}",
                new Dictionary<string, int> {{"A", 8}} );
        }

        [Fact]
        public void TestStringDictionaryWithUnknownComparer()
        {   
            // fallback to default comparer
            Assert.Throws<CaravelaException>( () =>
            {
                this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Object>{}",
                    new Dictionary<string, object>( StringComparer.Create( new CultureInfo( "sk-SK" ), false ) ) );
            } );
        }
        [Fact]
        public void TestStringDictionaryWithUnknownComparer2()
        {
            Assert.Throws<CaravelaException>( () =>
            {
                // fallback to default comparer
                this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.String, System.Object>{}",
                    new Dictionary<string, object>( new CustomComparer<string>() ) );
            } );
        }
        [Fact]
        public void TestIntDictionaryWithUnknownComparer()
        {
            Assert.Throws<CaravelaException>( () =>
            {
                // fallback to default comparer
                this.AssertSerialization( "new System.Collections.Generic.Dictionary<System.Int32, System.Object>{}",
                    new Dictionary<int, object>( new CustomComparer<int>() ) {{2, 8}} );
            } );
        }



        private void AssertSerialization( string expected, object o )
        {
            string creationExpression = this._serializer.SerializeObject(o).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }

    public class CustomComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T x, T y ) => true;

        public int GetHashCode( T obj ) => 0;
    }
}