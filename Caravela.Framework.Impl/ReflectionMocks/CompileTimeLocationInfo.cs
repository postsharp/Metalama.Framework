using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeLocationInfo : LocationInfo
    {
        public Property? Property { get; }

        public Field? Field { get; }

        public CompileTimeLocationInfo( Property property ) : base( (PropertyInfo) null! )
        {
            this.Property = property;
        }

        public CompileTimeLocationInfo( Field field ) : base( (FieldInfo) null! )
        {
            this.Field = field;
        }

        public static CompileTimeLocationInfo Create( IProperty property )
        {
            if ( property is Property trueProperty )
            {
                return new CompileTimeLocationInfo( trueProperty );
            }
            else
            {
                return new CompileTimeLocationInfo( (property as Field)! );
            }
        }
    }
}