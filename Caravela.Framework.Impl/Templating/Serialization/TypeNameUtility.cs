using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class TypeNameUtility
    {
        public static string ToCSharpQualifiedName( Type type )
        {
            if ( type.IsGenericParameter )
            {
                return type.Name;
            }
            if ( type.DeclaringType != null )
            {
                return ToCSharpQualifiedName( type.DeclaringType ) + "." + ToCSharpSimpleName( type );
            }
            else
            {
                return type.Namespace + "." + ToCSharpSimpleName( type );
            }
        }

        private static string ToCSharpSimpleName( Type type )
        {
            // TODO This entire class doesn't handle nested generic types well and possibly other stuff
            // TODO This method is weird as well
            // To be replaced with something from Roslyn, perhaps?
            if ( type.IsGenericType && type.GetGenericArguments().Length > 0) 
            {
                string shortName = type.Name;
                if (shortName.Contains('`')) shortName = shortName.Substring(0, shortName.IndexOf("`", StringComparison.Ordinal ));
                return shortName + "<" + string.Join( ",", type.GetGenericArguments().Select( ToCSharpQualifiedName ) )  + ">";
            }
            else
            {
                return type.Name;
            }
        }
    }
}