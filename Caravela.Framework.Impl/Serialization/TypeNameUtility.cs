// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// Has <see cref="ToCSharpQualifiedName"/>.
    /// </summary>
    internal static class TypeNameUtility
    {
        // TODO Multidimensional arrays, such as "int[,]", are not well-supported.

        /// <summary>
        /// Returns the fully-qualified name of a type as it would be written in C#, rather than the CLR name. Supports nested types and generics.
        /// </summary>
        /// <param name="type">A type, such as "int".</param>
        /// <returns>The type's fully qualified name, such as "System.Int32".</returns>
        public static string ToCSharpQualifiedName( Type type )
        {
            return GetCSharpTypeName( type );
        }

        /// <summary>
        ///     Gets the CS Type Code for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">type.</exception>
        private static string GetCSharpTypeName( this Type type )
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

            if ( type.IsArray )
            {
                var arrayLength = new List<string>();

                for ( var i = 0; i < type.GetArrayRank(); i++ )
                {
                    arrayLength.Add( "[]" );
                }

                return GetCSharpTypeName( type.GetElementType() ) + string.Join( "", arrayLength ).Replace( "+", "." );
            }

            return type.FullName.Replace( "+", "." );
        }

        private static string ToCSharpReservedWord( this Type type, bool fullName )
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

                return type.Name;
            }
        }

        private static string ToGenericTypeString( this Type t, params Type[] arg )
        {
            if ( t.IsGenericParameter || t.FullName == null )
            {
                return t.Name; // Generic argument stub
            }

            var isGeneric = t.IsGenericType
                            || t.FullName.IndexOf( '`' )
                            >= 0; // an array of generic types is not considered a generic type although it still have the genetic notation

            var isArray = !t.IsGenericType && t.FullName.IndexOf( '`' ) >= 0;
            var genericType = t;

            while ( genericType.IsNested && genericType.DeclaringType.GetGenericArguments().Length == t.GetGenericArguments().Length )
            {
                // Non generic class in a generic class is also considered in Type as being generic

                genericType = genericType.DeclaringType;
            }

            if ( !isGeneric )
            {
                return ToCSharpReservedWord( t, true ).Replace( '+', '.' );
            }

            var
                arguments = arg.Any()
                    ? arg
                    : t.GetGenericArguments(); // if arg has any then we are in the recursive part, note that we always must take arguments from t, since only t (the last one) will actually have the constructed type arguments and all others will just contain the generic parameters

            var genericTypeName = genericType.ToCSharpReservedWord( true );

            if ( genericType.IsNested )
            {
                var argumentsToPass = arguments.Take( genericType.DeclaringType.GetGenericArguments().Length )
                    .ToArray(); // Only the innermost will return the actual object and only from the GetGenericArguments directly on the type, not on the on genericDefinition, and only when all parameters including of the innermost are set

                arguments = arguments.Skip( argumentsToPass.Length ).ToArray();

                genericTypeName =
                    genericType.DeclaringType.ToGenericTypeString( argumentsToPass ) + "." + ToCSharpReservedWord( genericType, false ); // Recursive
            }

            if ( isArray )
            {
                genericTypeName = t.GetElementType().ToGenericTypeString() + "[]"; // this should work even for multidimensional arrays
            }

            if ( genericTypeName.IndexOf( '`' ) >= 0 )
            {
                genericTypeName = genericTypeName.Substring( 0, genericTypeName.IndexOf( '`' ) );
                var genericArgs = string.Join( ", ", arguments.Select( a => a.ToGenericTypeString() ).ToArray() );

                // Recursive
                genericTypeName = genericTypeName + "<" + genericArgs + ">";

                if ( isArray )
                {
                    genericTypeName += "[]";
                }
            }

            if ( t != genericType )
            {
                genericTypeName += t.FullName.Replace( genericType.ToCSharpReservedWord( true ), "" ).Replace( '+', '.' );
            }

            if ( genericTypeName.IndexOf( '[' ) >= 0 && genericTypeName.IndexOf( ']' ) != genericTypeName.IndexOf( '[' ) + 1 )
            {
                genericTypeName = genericTypeName.Substring(
                    0,
                    genericTypeName.IndexOf( '[' ) ); // For a non generic class nested in a generic class we will still have the type parameters at the end
            }

            return genericTypeName;
        }
    }
}