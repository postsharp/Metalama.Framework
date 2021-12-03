// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class SerializationReader
    {
        private const byte _version = 1;

        private readonly Dictionary<int, SerializationQueueItem<ObjRef>> _referenceTypeInstances = new Dictionary<int, SerializationQueueItem<ObjRef>>();

        private readonly SerializationBinaryReader _binaryReader;

        private readonly MetaFormatter _formatter;
        private readonly bool _shouldReportExceptionCause;

        internal SerializationReader( Stream stream, MetaFormatter formatter, bool shouldReportExceptionCause )
        {
            this._formatter = formatter;
            this._shouldReportExceptionCause = shouldReportExceptionCause;
            this._binaryReader = new SerializationBinaryReader( new BinaryReader( stream ) );
        }

        public object Deserialize()
        {
            int v = this._binaryReader.ReadCompressedInteger();
            if ( v > _version )
            {
                throw new NotSupportedException( "Unsupported formatter version!" );
            }
            
            var instanceId = 1;
            var rootObject = this.ReadObject( instanceId, true, null );
            //TODO: Consider refactoring. Should actually call read type and then decide whether to read object or call ReadValue.
            //But that's a lot of work. For now GetObjRef has a check to ignore instanceId for value types.

            for ( instanceId++; instanceId <= this._referenceTypeInstances.Count; instanceId++ )
            {
                this.InitializeObject( instanceId );
            }

            ISerializationCallback? callback;
            if ((callback = rootObject as ISerializationCallback) != null)
            {
                callback.OnDeserialized();
            }

            foreach ( var obj in this._referenceTypeInstances.Values )
            {
                if ((callback = obj.Value.Value as ISerializationCallback) != null && !ReferenceEquals( callback, rootObject ))
                {
                    callback.OnDeserialized();
                }
            }

            return rootObject;
        }

        private object ReadObject( int instanceId, bool initializeObject, SerializationCause cause )
        {
            if ( this._referenceTypeInstances.TryGetValue( instanceId, out var item ) )
                return item.Value.Value;

            var objRef = this.GetObjRef( instanceId, cause );

            return this.ReadObjectInternal( objRef, instanceId, initializeObject );
        }

        private object ReadObjectInternal( ObjRef objRef, int instanceId, bool initializeObject )
        {
            if ( objRef.Value == null )
            {
                return null;
            }

            // object can be ValueType so we need to check IsInitialized
            if ( !objRef.IsInitialized && initializeObject )
            {
                this.InitializeObject( instanceId );
            }

            return objRef.Value;
        }

        private void InitializeObject( int instanceId )
        {
            var item = this._referenceTypeInstances[instanceId];

            var objRef = item.Value;
            
            // object could be initialized in constructionData block
            if ( objRef.IsInitialized )
            {
                return;
            }

            objRef.IsInitialized = true;

            var type = objRef.Value.GetType();
            if ( type.IsArray )
            {
                this.ReadArray( (Array)objRef.Value, item.Cause );
            }
            else
            {
                if ( objRef.IntrinsicType == SerializationIntrinsicType.Class )
                {
                    var fields = this.ReadInstanceFields( type, false, item.Cause );

                    if ( objRef.Serializer.IsTwoPhase )
                    {
                        this.TryDeserializeFields( objRef.Serializer, ref objRef.Value, fields, item.Cause );
                    }
                }
                else
                {
                    // We have a primitive type.
                }
            }
        }

        private void TryDeserializeFields( IMetaSerializer serializer, ref object value, InstanceFields fields, SerializationCause cause )
        {
            try
            {
                serializer.DeserializeFields( ref value, fields );
            }
            catch ( MetaSerializationException exception )
            {
                throw MetaSerializationException.CreateWithCause( "Deserialization", value.GetType(), exception, cause );
            }
        }

        private InstanceFields ReadInstanceFields( Type type, bool initializeObjects, SerializationCause cause )
        {
            int fieldCount = this._binaryReader.ReadCompressedInteger();

            if ( fieldCount == 0 )
            {
                return InstanceFields.Empty;
            }

            var fields = new InstanceFields( type, this._formatter, fieldCount );

            for ( var i = 0; i < fieldCount; i++ )
            {
                string fieldName = this._binaryReader.ReadDottedString();

                var newCause = this._shouldReportExceptionCause ? SerializationCause.WithTypedValue( cause, fieldName, type ) : null;
                var value = this.ReadTypedValue( initializeObjects, newCause );

                fields.Values.Add( fieldName, value );
            }

            return fields;
        }

        private void ReadType( out Type type, out SerializationIntrinsicType intrinsicType )
        {
            intrinsicType = (SerializationIntrinsicType)this._binaryReader.ReadByte();

            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.None:
                    type = null;
                    break;

                case SerializationIntrinsicType.Byte:
                    type = typeof(byte);
                    break;

                case SerializationIntrinsicType.SByte:
                    type = typeof(sbyte);
                    break;

                case SerializationIntrinsicType.Int16:
                    type = typeof(short);
                    break;

                case SerializationIntrinsicType.Int32:
                    type = typeof(int);
                    break;

                case SerializationIntrinsicType.Int64:
                    type = typeof(long);
                    break;

                case SerializationIntrinsicType.UInt16:
                    type = typeof(ushort);
                    break;

                case SerializationIntrinsicType.UInt32:
                    type = typeof(uint);
                    break;

                case SerializationIntrinsicType.UInt64:
                    type = typeof(ulong);
                    break;

                case SerializationIntrinsicType.Single:
                    type = typeof(float);
                    break;

                case SerializationIntrinsicType.Double:
                    type = typeof(double);
                    break;

                case SerializationIntrinsicType.String:
                    type = typeof(string);
                    break;

                case SerializationIntrinsicType.DottedString:
                    type = typeof(DottedString);
                    break;

                case SerializationIntrinsicType.Boolean:
                    type = typeof(bool);
                    break;

                case SerializationIntrinsicType.Enum:
                    type = this.ReadNamedType();
                    if ( !type.IsEnum )
                    {
                        throw new MetaSerializationException( string.Format(CultureInfo.InvariantCulture, "Type '{0}' is expected to be an enum type.", type ) );
                    }
                    break;

                case SerializationIntrinsicType.Struct:
                    type = this.ReadNamedType();
                    if ( !type.IsValueType )
                    {
                        throw new MetaSerializationException( string.Format(CultureInfo.InvariantCulture, "Type '{0}' is expected to be a value type.", type ) );
                    }
                    break;

                case SerializationIntrinsicType.Class:
                    type = this.ReadNamedType();
                    if ( type.IsValueType )
                    {
                        throw new MetaSerializationException( string.Format(CultureInfo.InvariantCulture, "Type '{0}' is expected to be a reference type.", type ) );
                    }
                    break;

                
                case SerializationIntrinsicType.Array:
                    int rank = this._binaryReader.ReadCompressedInteger();
                    Type elementType;
                    SerializationIntrinsicType elementIntrinsicType;
                    this.ReadType( out elementType, out elementIntrinsicType );
                    type = rank == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType( rank );
                    break;

                case SerializationIntrinsicType.Char:
                    type = typeof(char);
                    break;

                case SerializationIntrinsicType.ObjRef:
                    type = typeof(object);
                    break;

                case SerializationIntrinsicType.Type:
                    type = typeof(Type);
                    break;

                case SerializationIntrinsicType.GenericTypeParameter:
                    type = this.ReadGenericTypeParameter();
                    break;

                case SerializationIntrinsicType.GenericMethodParameter:
                    type = this.ReadGenericMethodParameter();
                    break;

                default:
                    throw new MetaSerializationException($"Invalid type: {intrinsicType}.");
            }

         
        }

        private Type ReadNamedType()
        {
            var flags = (SerializationIntrinsicTypeFlags) this._binaryReader.ReadByte();
            switch ( flags )
            {
                case SerializationIntrinsicTypeFlags.Default:
                    {
                        var typeName = this.ReadTypeName();
                        return this.GetType( typeName );
                    }

                case SerializationIntrinsicTypeFlags.Generic:
                    {
                        var typeName = this.ReadTypeName();
                        var genericType = this.GetType(typeName);
                        int arity = this._binaryReader.ReadCompressedInteger();

                        if (arity > 0)
                        {
                            var genericArguments = new Type[arity];
                            for (var i = 0; i < arity; i++)
                            {
                                genericArguments[i] = this.ReadType();
                            }
                            return genericType.MakeGenericType(genericArguments);
                        }
                        else
                        {
                            return genericType;
                        }
                    }

                    // TODO: Remove
                    //case SerializationIntrinsicTypeFlags.MetadataIndex:
                    //{
                    //    int index = this.binaryReader.ReadCompressedInteger();
                    //    return (Type) this.formatter.MetadataDispenser.GetMetadata( index );
                    //}

                default:
                    throw new MetaSerializationException();
            }
            
        }


        private Type ReadType()
        {
            Type type;
            SerializationIntrinsicType intrinsicType;
            this.ReadType( out type, out intrinsicType );
            return type;
        }

        private object ReadTypedValue( bool initializeObjects, SerializationCause cause )
        {
            SerializationIntrinsicType intrinsicType;
            Type type;

            this.ReadType( out type, out intrinsicType );

            if ( type == null )
            {
                return null;
            }

            var value = this.ReadValue( intrinsicType, type, initializeObjects, cause );
            return value;
        }

        private object ReadValue( SerializationIntrinsicType intrinsicType, Type type, bool initializeObject, SerializationCause cause )
        {
            if ( intrinsicType == SerializationIntrinsicType.None )
            {
                intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( type );
            }

            object value;
            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.Byte:
                    value = this._binaryReader.ReadByte();
                    break;

                case SerializationIntrinsicType.SByte:
                    value = this._binaryReader.ReadSByte();
                    break;

                case SerializationIntrinsicType.Int16:
                    value = (short)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.Int32:
                    value = (int)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.Int64:
                    value = (long)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.UInt16:
                    value = (ushort)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.UInt32:
                    value = (uint)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.UInt64:
                    value = (ulong)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.Single:
                    value = this._binaryReader.ReadSingle();
                    break;

                case SerializationIntrinsicType.Double:
                    value = this._binaryReader.ReadDouble();
                    break;

                case SerializationIntrinsicType.String:
                    value = this._binaryReader.ReadString();
                    break;

                case SerializationIntrinsicType.DottedString:
                    value = this._binaryReader.ReadDottedString();
                    break;

                case SerializationIntrinsicType.Boolean:
                    value = this._binaryReader.ReadByte() != 0;
                    break;

                case SerializationIntrinsicType.Struct:
                    value = this.ReadStruct( type, cause );
                    break;

                case SerializationIntrinsicType.ObjRef:
                    value = this.ReadObjRef( initializeObject, cause );
                    break;

                case SerializationIntrinsicType.Char:
                    value = (char)this._binaryReader.ReadCompressedInteger();
                    break;

                case SerializationIntrinsicType.Type:
                    value = this.ReadType();
                    break;

                case SerializationIntrinsicType.Enum:
                    var enumValue = this._binaryReader.ReadCompressedInteger();
                    // explicite cast is needed due to check in Enum.ToObject (it throws if type is not numeric type)
                    value = enumValue.IsNegative ? Enum.ToObject( type, (long)enumValue ) : Enum.ToObject( type, (ulong)enumValue );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(intrinsicType));
            }
            return value;
        }

        private Type ReadGenericTypeParameter()
        {
            var declaringType = this.ReadType();
            int position = this._binaryReader.ReadCompressedInteger();
            return declaringType.GetGenericArguments()[position];
        }

        private Type ReadGenericMethodParameter()
        {
            var declaringMethod = (MethodInfo)this.ReadMethod();
            int position = this._binaryReader.ReadCompressedInteger();
            return declaringMethod.GetGenericArguments()[position];
        }

        private MethodBase ReadMethod()
        {
            var declaringType = this.ReadType();
            var methodName = this._binaryReader.ReadString();
            var methodSignature = this._binaryReader.ReadString();

            throw new NotSupportedException();
            // TODO: Remove this method.
            //return ReflectionHelper.GetMethod( methodName, methodSignature );
        }

        private Type GetType( AssemblyTypeName typeName )
        {
            return this._formatter.Binder.BindToType( typeName.TypeName, typeName.AssemblyName );
        }

        private void ReadArray( Array array, SerializationCause cause )
        {
            var indices = new int[array.Rank];

            this.ReadArrayElements( array, array.GetType().GetElementType(), indices, 0, cause );
        }

        private void ReadArrayElements( Array array, Type elementType, int[] indices, int currentDimension, SerializationCause cause )
        {
            var length = array.GetLength( currentDimension );
            var lowerBound = array.GetLowerBound( currentDimension );

            if ( currentDimension + 1 < indices.Length )
            {
                for ( var i = lowerBound; i < lowerBound + length; i++ )
                {
                    indices[currentDimension] = i;
                    this.ReadArrayElements( array, elementType, indices, currentDimension + 1, cause );
                }
            }
            else
            {
                var elementIntrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( elementType, true );

                for ( var i = lowerBound; i < lowerBound + length; i++ )
                {
                    indices[currentDimension] = i;

                    var newCause = this._shouldReportExceptionCause ? SerializationCause.WithIndices( cause, indices ) : null;
                    if ( SerializationIntrinsicTypeExtensions.IsPrimitiveIntrinsic( elementIntrinsicType ) )
                    {
                        array.SetValue( this.ReadValue( elementIntrinsicType, elementType, false, newCause ), indices );
                    }
                    else
                    {
                        array.SetValue( this.ReadTypedValue( false, newCause ), indices );
                    }
                }
            }
        }

        private object ReadObjRef( bool initializeObject, SerializationCause cause )
        {
            int instanceId = this._binaryReader.ReadCompressedInteger();

            return this.ReadObject( instanceId, initializeObject, cause );
        }

        private ObjRef GetObjRef( int instanceId, SerializationCause cause )
        {
            if ( this._referenceTypeInstances.TryGetValue( instanceId, out var item ) )
                return item.Value;

            // Create an uninitialized instance for this type.
            this.ReadType( out var type, out var intrinsicType );

            if ( cause == null && this._shouldReportExceptionCause )
                // This is the root.
                cause = SerializationCause.WithTypedValue( null, "root", type );

            if ( type == null )
                return ObjRef.Empty;

            object value;
            IMetaSerializer serializer;
            if ( intrinsicType == SerializationIntrinsicType.Array )
            {
                var lengths = new int[type.GetArrayRank()];
                var lowerBounds = new int[type.GetArrayRank()];
                for ( var i = 0; i < lengths.Length; i++ )
                {
                    lengths[i] = this._binaryReader.ReadCompressedInteger();
                    lowerBounds[i] = this._binaryReader.ReadCompressedInteger();
                }

                value = Array.CreateInstance( type.GetElementType(), lengths, lowerBounds );

                serializer = null;
            }
            else if ( intrinsicType == SerializationIntrinsicType.Class || intrinsicType == SerializationIntrinsicType.Struct )
            {
                var fields = this.ReadInstanceFields( type, true, cause );
                serializer = this._formatter.SerializerProvider.GetSerializer( type );

                value = this.TryCreateInstance( serializer, type, fields, cause );
            }
            else
            {
                value = this.ReadValue( intrinsicType, type, true, cause );
                serializer = null;
            }

            var objRef = new ObjRef( value, serializer, intrinsicType );

            if ( !type.IsValueType )
            {
                this._referenceTypeInstances.Add( instanceId, new SerializationQueueItem<ObjRef>(objRef, cause));
            }
            else
            {
                // ValueTypes are always initialized
                objRef.IsInitialized = true;
            }

            return objRef;
        }

        private object TryCreateInstance( IMetaSerializer serializer, Type type, InstanceFields fields, SerializationCause cause )
        {
            try
            {
                return serializer.CreateInstance( type, fields );
            }
            catch ( MetaSerializationException exception )
            {
                throw MetaSerializationException.CreateWithCause( "Deserialization", type, exception, cause );
            }
        }

        private object ReadStruct( Type type, SerializationCause cause )
        {
            var fields = this.ReadInstanceFields( type, true, cause );

            var serializer = this._formatter.SerializerProvider.GetSerializer( type );

            var value = this.TryCreateInstance( serializer, type, fields, cause );
            
            this.TryDeserializeFields( serializer, ref value, fields, cause );

            return value;
        }

        private AssemblyTypeName ReadTypeName()
        {
            return new AssemblyTypeName( this._binaryReader.ReadDottedString(), this._binaryReader.ReadString() );
        }

        private sealed class InstanceFields : IArgumentsReader
        {
            public static readonly InstanceFields Empty = new InstanceFields();

            private readonly Type? type;

            public readonly Dictionary<string, object>? Values;

            private readonly MetaFormatter? formatter;

            private InstanceFields()
            {
                this.type = null;
                this.formatter = null;
                this.Values = null;
            }

            public InstanceFields( Type type, MetaFormatter formatter, int capacity )
            {
                this.type = type;
                this.formatter = formatter;
                this.Values = new Dictionary<string, object>( capacity, StringComparer.Ordinal );
            }

            public bool TryGetValue<T>( string name, [MaybeNullWhen( false )] out T value, string? scope = null )
            {
                if (this.Values == null)
                {
                    value = default(T)!;
                    return false;
                }

                object valueObj;

                if (scope != null)
                {
                    name = scope + "." + name;
                }

                if (!this.Values.TryGetValue(name, out valueObj))
                {
                    value = default(T);
                    return false;
                }

                if (valueObj == null)
                {
                    value = default(T);
                    return true;
                }

                IMetaSerializer serializer = null;

                if (!typeof(T).HasElementType)
                {
                    this.formatter.SerializerProvider.TryGetSerializer(typeof(T), out serializer);
                }

                try
                {
                    if (serializer != null)
                    {
                        value = (T)serializer.Convert(valueObj, typeof(T));
                    }
                    else
                    {
                        value = (T)valueObj;

                    }
                    return true;
                }
                catch (Exception e)
                {
                    #if LEGACY_REFLECTION_API
                    Type GetElementType(Type type)
                    {
                        if (type.HasElementType)
                        {
                            return type.GetElementType();
                        }
                        else if (type.GetTypeDefinition() == typeof(Nullable<>))
                        {
                            return type.GetGenericArguments()[0];
                        }
                        else
                        {
                            return type;
                        }
                    }
#endif

                    string FormatTypeName(Type type)
                    {
#if LEGACY_REFLECTION_API
                        return type.AssemblyQualifiedName + " (" + GetElementType(type).Assembly.Location + ")";
#else
                        return type.AssemblyQualifiedName;
#endif
                    }


                    throw new MetaSerializationException(
                        string.Format(CultureInfo.InvariantCulture,
                            "Error reading value of key '{0}' in type '{1}': cannot convert type '{2}' into '{3}': {4}",
                            name,
                            this.type,
                            FormatTypeName( valueObj.GetType() ),
                            FormatTypeName( typeof(T) ),
                            e.Message),
                        e);
                }
            }

            public T GetValue<T>( string name, string? scope = null )
            {
                T value;
                this.TryGetValue( name, out value, scope );
                return value;
            }

            // TODO: Remove
            //public IMetadataDispenser MetadataDispenser { get { return this.formatter.MetadataDispenser;} }
        }

        private class ObjRef
        {
            public static readonly ObjRef Empty = new ObjRef();

            public object Value;

            public readonly SerializationIntrinsicType IntrinsicType;

            public readonly IMetaSerializer Serializer;

            public bool IsInitialized;

            private ObjRef()
            {
                this.IntrinsicType = SerializationIntrinsicType.None;
            }

            public ObjRef( object value, IMetaSerializer serializer, SerializationIntrinsicType intrinsicType )
            {
                this.Value = value;
                this.Serializer = serializer;
                this.IntrinsicType = intrinsicType;
                this.IsInitialized = false;
            }
        }
    }
}