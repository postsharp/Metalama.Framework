using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    [Obfuscation( Exclude = true )]
    internal class CaravelaLocationInfo : LocationInfo
    {
        public Property? Property { get; }
        public Field? Field { get; }

        public CaravelaLocationInfo(Property property) : base((PropertyInfo)null!) => this.Property = property;

        public CaravelaLocationInfo(Field field) : base((FieldInfo)null!) => this.Field = field;

        public static CaravelaLocationInfo Create( IProperty property )
        {
            if ( property is Property trueProperty )
            {
                return new CaravelaLocationInfo( trueProperty );
            }
            else
            {
                return new CaravelaLocationInfo( (property as Field)! );
            }
        }
    }
}