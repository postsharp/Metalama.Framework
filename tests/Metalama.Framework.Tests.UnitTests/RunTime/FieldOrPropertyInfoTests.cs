// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.RunTime;
using System.Reflection;
using Xunit;

#pragma warning disable SA1401 // Fields should be private

// Resharper disable UnassignedField.Global

namespace Metalama.Framework.Tests.UnitTests.RunTime
{
    public sealed class FieldOrPropertyInfoTests
    {
        [Fact]
        public void WithField()
        {
            var field = this.GetType().GetField( nameof(this.MyField) )!;
            var fieldOrPropertyInfo = new FieldOrPropertyInfo( field );
            Assert.Equal( fieldOrPropertyInfo.DeclaringType, this.GetType() );
            Assert.Equal( fieldOrPropertyInfo.ReflectedType, this.GetType() );
            Assert.Equal( MemberTypes.Field, fieldOrPropertyInfo.MemberType );
            Assert.Equal( nameof(this.MyField), fieldOrPropertyInfo.Name );
            Assert.Equal( typeof(int), fieldOrPropertyInfo.ValueType );
            Assert.Equal( field, fieldOrPropertyInfo.AsField );
            Assert.Equal( field, fieldOrPropertyInfo.UnderlyingMemberInfo );

            fieldOrPropertyInfo.SetValue( this, 5 );
            Assert.Equal( 5, fieldOrPropertyInfo.GetValue( this ) );
        }

        [Fact]
        public void WithProperty()
        {
            var property = this.GetType().GetProperty( nameof(this.MyProperty) )!;
            var fieldOrPropertyInfo = new FieldOrPropertyInfo( property );
            Assert.Equal( fieldOrPropertyInfo.DeclaringType, this.GetType() );
            Assert.Equal( fieldOrPropertyInfo.ReflectedType, this.GetType() );
            Assert.Equal( MemberTypes.Property, fieldOrPropertyInfo.MemberType );
            Assert.Equal( typeof(int), fieldOrPropertyInfo.ValueType );
            Assert.Equal( nameof(this.MyProperty), fieldOrPropertyInfo.Name );
            Assert.Equal( property, fieldOrPropertyInfo.AsPropertyOrIndexer );
            Assert.Equal( property, fieldOrPropertyInfo.UnderlyingMemberInfo );

            fieldOrPropertyInfo.SetValue( this, 5 );
            Assert.Equal( 5, fieldOrPropertyInfo.GetValue( this ) );
        }

        public int MyField;

        public int MyProperty { get; set; }
    }
}