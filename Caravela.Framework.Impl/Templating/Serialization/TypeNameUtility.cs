using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public static class TypeNameUtility
    {
        public static string ToCSharpQualifiedName( Type type )
        {
            return GetCSTypeName( type );
        }

        /// <summary>
        ///     Gets the CS Type Code for a type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        private static string GetCSTypeName( this Type type )
        {
            // https://stackoverflow.com/a/45718771/1580088
            // if ( type == typeof(string) )
            // {
            //     return "string";
            // }
            // else if ( type == typeof(object) ) { return "object"; }
            // else if ( type == typeof(bool) ) { return "bool"; }
            // else if ( type == typeof(char) ) { return "char"; }
            // else if ( type == typeof(int) ) { return "int"; }
            // else if ( type == typeof(float) ) { return "float"; }
            // else if ( type == typeof(double) ) { return "double"; }
            // else if ( type == typeof(long) ) { return "long"; }
            // else if ( type == typeof(ulong) ) { return "ulong"; }
            // else if ( type == typeof(uint) ) { return "uint"; }
            // else if ( type == typeof(byte) ) { return "byte"; }
            // else if ( type == typeof(Int64) ) { return "Int64"; }
            // else if ( type == typeof(short) ) { return "short"; }
            // else if ( type == typeof(decimal) ) { return "decimal"; }
            if ( type.IsGenericType )
            {
                return $"{ToGenericTypeString( type )}";
            }
            else if ( type.IsArray )
            {
                List<string> arrayLength = new List<string>();
                for ( int i = 0; i < type.GetArrayRank(); i++ )
                {
                    arrayLength.Add( "[]" );
                }

                return GetCSTypeName( type.GetElementType() ) + string.Join( "", arrayLength ).Replace( "+", "." );
            }
            else
            {
                return type.FullName.Replace( "+", "." );
            }
        }

        private static string ToCSReservatedWord( this Type type, bool fullName )
        {
            // if ( type == typeof(string) )
            // {
            //     return "string";
            // }
            // else if ( type == typeof(object) ) { return "object"; }
            // else if ( type == typeof(bool) ) { return "bool"; }
            // else if ( type == typeof(char) ) { return "char"; }
            // else if ( type == typeof(int) ) { return "int"; }
            // else if ( type == typeof(float) ) { return "float"; }
            // else if ( type == typeof(double) ) { return "double"; }
            // else if ( type == typeof(long) ) { return "long"; }
            // else if ( type == typeof(ulong) ) { return "ulong"; }
            // else if ( type == typeof(uint) ) { return "uint"; }
            // else if ( type == typeof(byte) ) { return "byte"; }
            // else if ( type == typeof(Int64) ) { return "Int64"; }
            // else if ( type == typeof(short) ) { return "short"; }
            // else if ( type == typeof(decimal) ) { return "decimal"; }
            // else
            {
                if ( fullName )
                {
                    return type.FullName;
                }
                else
                {
                    return type.Name;
                }

            }
        }

        private static string ToGenericTypeString( this Type t, params Type[] arg )
        {
            if ( t.IsGenericParameter || t.FullName == null )
            {
                return t.Name; // Generic argument stub
            }

            bool isGeneric = t.IsGenericType || t.FullName.IndexOf( '`' ) >= 0; //an array of generic types is not considered a generic type although it still have the genetic notation
            bool isArray = !t.IsGenericType && t.FullName.IndexOf( '`' ) >= 0;
            Type genericType = t;
            while ( genericType.IsNested && genericType.DeclaringType.GetGenericArguments().Count() == t.GetGenericArguments().Count() ) //Non generic class in a generic class is also considered in Type as being generic
            {
                genericType = genericType.DeclaringType;
            }

            if ( !isGeneric ) return ToCSReservatedWord( t, true ).Replace( '+', '.' );

            var
                arguments = arg.Any()
                    ? arg
                    : t.GetGenericArguments(); //if arg has any then we are in the recursive part, note that we always must take arguments from t, since only t (the last one) will actually have the constructed type arguments and all others will just contain the generic parameters
            string genericTypeName = genericType.ToCSReservatedWord( true );
            if ( genericType.IsNested )
            {
                var argumentsToPass = arguments.Take( genericType.DeclaringType.GetGenericArguments().Count() )
                    .ToArray(); //Only the innermost will return the actual object and only from the GetGenericArguments directly on the type, not on the on genericDfintion, and only when all parameters including of the innermost are set
                arguments = arguments.Skip( argumentsToPass.Count() ).ToArray();
                genericTypeName = genericType.DeclaringType.ToGenericTypeString( argumentsToPass ) + "." + ToCSReservatedWord( genericType, false ); //Recursive
            }

            if ( isArray )
            {
                genericTypeName = t.GetElementType().ToGenericTypeString() + "[]"; //this should work even for multidimensional arrays
            }

            if ( genericTypeName.IndexOf( '`' ) >= 0 )
            {
                genericTypeName = genericTypeName.Substring( 0, genericTypeName.IndexOf( '`' ) );
                string genericArgs = string.Join( ", ", arguments.Select( a => a.ToGenericTypeString() ).ToArray() );
                //Recursive
                genericTypeName = genericTypeName + "<" + genericArgs + ">";
                if ( isArray ) genericTypeName += "[]";
            }

            if ( t != genericType )
            {
                genericTypeName += t.FullName.Replace( genericType.ToCSReservatedWord( true ), "" ).Replace( '+', '.' );
            }

            if ( genericTypeName.IndexOf( '[' ) >= 0 && genericTypeName.IndexOf( ']' ) != genericTypeName.IndexOf( '[' ) + 1 )
                genericTypeName = genericTypeName.Substring( 0, genericTypeName.IndexOf( '[' ) ); //For a non generic class nested in a generic class we will still have the type parameters at the end 
            return genericTypeName;
        }
    }
}