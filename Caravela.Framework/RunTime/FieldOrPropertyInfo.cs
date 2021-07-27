// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;

namespace Caravela.Framework.RunTime
{
    /// <summary>
    /// Represents a reflection <see cref="FieldInfo"/> or a <see cref="PropertyInfo"/>. 
    /// </summary>
    public class FieldOrPropertyInfo : MemberInfo
    {
        private readonly MemberInfo? _underlyingMemberInfo;

        public MemberInfo UnderlyingMemberInfo
            => this._underlyingMemberInfo ?? throw new InvalidOperationException( "This object cannot be accessed at compile time." );

        /// <summary>
        /// Gets the <see cref="FieldInfo"/> if this represents a field, otherwise returns null.
        /// </summary>
        public FieldInfo? AsFieldInfo => (FieldInfo?) this.UnderlyingMemberInfo;

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> if this represents a property, otherwise returns null.
        /// </summary>
        public PropertyInfo? AsPropertyInfo => (PropertyInfo?) this.UnderlyingMemberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldOrPropertyInfo"/> class that represents a field.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        public FieldOrPropertyInfo( FieldInfo fieldInfo )
        {
            this._underlyingMemberInfo = fieldInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldOrPropertyInfo"/> class that represents a property.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <param name="propertyInfo">The property.</param>
        public FieldOrPropertyInfo( PropertyInfo propertyInfo )
        {
            this._underlyingMemberInfo = propertyInfo;
        }

        // Compile-time constructor.
        private protected FieldOrPropertyInfo() { }

        public override object[] GetCustomAttributes( bool inherit ) => this.UnderlyingMemberInfo.GetCustomAttributes( inherit );

        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
            => this.UnderlyingMemberInfo.GetCustomAttributes( attributeType, inherit );

        public override bool IsDefined( Type attributeType, bool inherit ) => this.UnderlyingMemberInfo.IsDefined( attributeType, inherit );

        public override Type DeclaringType => this.UnderlyingMemberInfo.DeclaringType!;

        public override MemberTypes MemberType => this.UnderlyingMemberInfo.MemberType;

        public override string Name => this.UnderlyingMemberInfo.Name;

        public override Type ReflectedType => this.UnderlyingMemberInfo.ReflectedType!;

        public object? GetValue( object? obj )
        {
            switch ( this.UnderlyingMemberInfo )
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue( obj );

                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue( obj );

                default:
                    throw new InvalidOperationException();
            }
        }

        public void SetValue( object? obj, object? value )
        {
            switch ( this.UnderlyingMemberInfo )
            {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue( obj, value );

                    break;

                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue( obj, value );

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}