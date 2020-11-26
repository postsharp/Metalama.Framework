using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaLocationInfo : LocationInfo
    {
        public Property? Property { get; }
        public Field? Field { get; }

        public CaravelaLocationInfo(Property property) : base((PropertyInfo)null!) => this.Property = property;

        public CaravelaLocationInfo(Field field) : base((FieldInfo)null!) => this.Field = field;
    }
}