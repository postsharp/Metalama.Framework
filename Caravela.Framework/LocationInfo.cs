using System.Reflection;

namespace Caravela.Framework
{
    public class LocationInfo
    {
        public FieldInfo FieldInfo { get; }
        public PropertyInfo PropertyInfo { get; }

        public LocationInfo( FieldInfo fieldInfo )
        {
            this.FieldInfo = fieldInfo;
        }

        public LocationInfo( PropertyInfo propertyInfo )
        {
            this.PropertyInfo = propertyInfo;
        }
    }
}