using Caravela.Framework.Code;
using System.Linq;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public static class CodeModelUtilities
    {
        public static IMethod Method( this INamedType type, string name )
        {
            return type.AllMethods.GetValue().Single( m => m.Name == name );
        }  
        public static IProperty Property( this INamedType type, string name )
        {
            return type.AllProperties.GetValue().Single( m => m.Name == name );
        }
        public static IEvent Event( this INamedType type, string name )
        {
            return type.AllEvents.GetValue().Single( m => m.Name == name );
        }
    }
}