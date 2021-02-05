using System.Reflection;

namespace Caravela.Framework
{
    /// <summary>
    /// Represents a field or a property. Only one of the properties of this class is ever used.
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// If this represents a field, returns the <see cref="FieldInfo"/>, otherwise returns null.
        /// </summary>
        public FieldInfo? FieldInfo { get; }

        /// <summary>
        /// If this represents a property, returns the <see cref="PropertyInfo"/>, otherwise returns null.
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