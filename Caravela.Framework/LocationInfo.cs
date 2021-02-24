// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Caravela.Framework
{
    /// <summary>
    /// Represents a field or a property. Only one of the properties of this class is ever used.
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// Gets the <see cref="FieldInfo"/> if this represents a field, otherwise returns null.
        /// </summary>
        public FieldInfo? FieldInfo { get; }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> if this represents a property, otherwise returns null.
        /// </summary>
        public PropertyInfo? PropertyInfo { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationInfo"/> class that represents a field.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        public LocationInfo( FieldInfo fieldInfo )
        {
            this.FieldInfo = fieldInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationInfo"/> class that represents a property.
        /// </summary>
        /// <param name="fieldInfo">The field.</param>
        /// <param name="propertyInfo">The property.</param>
        public LocationInfo( PropertyInfo propertyInfo )
        {
            this.PropertyInfo = propertyInfo;
        }
    }
}