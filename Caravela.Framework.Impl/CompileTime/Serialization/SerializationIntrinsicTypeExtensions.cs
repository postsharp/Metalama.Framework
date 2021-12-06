// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal static class SerializationIntrinsicTypeExtensions
    {
        public static bool IsPrimitiveIntrinsic( SerializationIntrinsicType intrinsicType )
        {
            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.Byte:
                case SerializationIntrinsicType.Int16:
                case SerializationIntrinsicType.UInt16:
                case SerializationIntrinsicType.Int32:
                case SerializationIntrinsicType.UInt32:
                case SerializationIntrinsicType.Int64:
                case SerializationIntrinsicType.UInt64:
                case SerializationIntrinsicType.Single:
                case SerializationIntrinsicType.Double:
                case SerializationIntrinsicType.DottedString:
                case SerializationIntrinsicType.Char:
                case SerializationIntrinsicType.SByte:
                case SerializationIntrinsicType.Struct:
                case SerializationIntrinsicType.Enum:
                case SerializationIntrinsicType.Boolean:
                    return true;

                case SerializationIntrinsicType.String:
                case SerializationIntrinsicType.Class:
                case SerializationIntrinsicType.Array:
                case SerializationIntrinsicType.ObjRef:
                case SerializationIntrinsicType.Type:
                case SerializationIntrinsicType.GenericTypeParameter:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException( nameof( intrinsicType ) );
            }
        }

        public static SerializationIntrinsicType GetIntrinsicType( Type type, bool useObjRef = false )
        {
            if ( type == null )
            {
                return SerializationIntrinsicType.None;
            }

            if ( type.IsEnum )
            {
                return SerializationIntrinsicType.Enum;
            }

            switch ( Type.GetTypeCode( type ) )
            {
                case TypeCode.Boolean:
                    return SerializationIntrinsicType.Boolean;
                case TypeCode.Char:
                    return SerializationIntrinsicType.Char;
                case TypeCode.SByte:
                    return SerializationIntrinsicType.SByte;
                case TypeCode.Byte:
                    return SerializationIntrinsicType.Byte;
                case TypeCode.Int16:
                    return SerializationIntrinsicType.Int16;
                case TypeCode.UInt16:
                    return SerializationIntrinsicType.UInt16;
                case TypeCode.Int32:
                    return SerializationIntrinsicType.Int32;
                case TypeCode.UInt32:
                    return SerializationIntrinsicType.UInt32;
                case TypeCode.Int64:
                    return SerializationIntrinsicType.Int64;
                case TypeCode.UInt64:
                    return SerializationIntrinsicType.UInt64;
                case TypeCode.Single:
                    return SerializationIntrinsicType.Single;
                case TypeCode.Double:
                    return SerializationIntrinsicType.Double;
                case TypeCode.Decimal:
                    return SerializationIntrinsicType.Struct;
                case TypeCode.DateTime:
                    return SerializationIntrinsicType.Struct;
                case TypeCode.String:
                    return SerializationIntrinsicType.String;
                case TypeCode.Object:
                    if ( type.IsGenericParameter )
                    {
                        if ( type.DeclaringMethod != null )
                        {
                            // Not supported.
                            throw new AssertionFailedException();
                        }
                        else
                        {
                            return SerializationIntrinsicType.GenericTypeParameter;
                        }
                    }
                    else if ( type.IsValueType )
                    {
                        if ( type == typeof( DottedString ) )
                        {
                            return SerializationIntrinsicType.DottedString;
                        }
                        else
                        {
                            return SerializationIntrinsicType.Struct;
                        }
                    }
                    else
                    {
                        if ( typeof( Type ).IsAssignableFrom( type ) )
                        {
                            return SerializationIntrinsicType.Type;
                        }
                        else
                        {
                            if ( useObjRef )
                            {
                                return SerializationIntrinsicType.ObjRef;
                            }
                            else if ( type.IsArray )
                            {
                                return SerializationIntrinsicType.Array;
                            }
                            else
                            {
                                return SerializationIntrinsicType.Class;
                            }
                        }
                    }

                case TypeCode.DBNull:
                    return SerializationIntrinsicType.Struct;

                default:
                    throw new ArgumentOutOfRangeException( nameof( type ) );
            }
        }
    }
}