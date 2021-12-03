// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal class SerializationWriter
    {
     
        private readonly SerializationBinaryWriter binaryWriter;

        private readonly Queue<SerializationQueueItem<object>> serializationQueue = new Queue<SerializationQueueItem<object>>();

        private readonly MetaFormatter formatter;
        private readonly bool shouldReportExceptionCause;

        private readonly Dictionary<Type, AssemblyTypeName> typeNameCache = new Dictionary<Type, AssemblyTypeName>();
        private readonly Dictionary<Type, Type> surrogateTypesCache = new Dictionary<Type, Type>();
        private readonly Dictionary<object, ObjectInfo> objects = new Dictionary<object, ObjectInfo>( new CanonicalComparer() );

        private const int version = 1;

        public SerializationWriter( Stream stream, MetaFormatter formatter, bool shouldReportExceptionCause )
        {
            this.formatter = formatter;
            this.shouldReportExceptionCause = shouldReportExceptionCause;
            this.binaryWriter = new SerializationBinaryWriter( new BinaryWriter( stream ) );
        }

        public void Serialize( object obj )
        {
            this.binaryWriter.WriteCompressedInteger( version );

            var cause = this.shouldReportExceptionCause ? SerializationCause.WithTypedValue( null, "root", obj.GetType() ) : null;

            this.serializationQueue.Enqueue(
                new SerializationQueueItem<object>( obj, cause ) );

            while ( this.serializationQueue.Count > 0 )
            {
                var item = this.serializationQueue.Dequeue();

                this.WriteObject( item.Value, item.Cause );
            }

            this.binaryWriter.WriteByte( (byte)SerializationIntrinsicType.None );
        }

        private void CallOnSerialization( object obj )
        {
            ISerializationCallback callback;
            if ( (callback = obj as ISerializationCallback) != null )
            {
                callback.OnSerializing();
            }
        }

        private ObjectInfo GetObjectInfo( object obj, SerializationCause cause )
        {
            var type = obj.GetType();

            if ( type.IsValueType )
            {
                throw new ArgumentOutOfRangeException(nameof(obj));
            }

            ObjectInfo objectInfo;
            if ( !this.objects.TryGetValue( obj, out objectInfo ) )
            {
                this.CallOnSerialization(obj);

                var serializer = type.IsArray ? null : this.formatter.SerializerProvider.GetSerializer( type );

                objectInfo = new ObjectInfo( obj, this.objects.Count + 1, this.formatter );

                if ( !type.IsArray )
                    this.TrySerialize( serializer, obj, objectInfo.ConstructorArguments, objectInfo.InitializationArguments, cause );

                this.objects.Add( obj, objectInfo );
            }

            return objectInfo;
        }

        private void TrySerialize(IMetaSerializer serializer, object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments, SerializationCause cause )
        {
            try
            {
                serializer.SerializeObject( obj, constructorArguments, initializationArguments );
            }
            catch ( Exception exception )
            {
                throw MetaSerializationException.CreateWithCause( "Serialization", obj.GetType(), exception, cause );
            }
        }
        
        private void WriteType( Type type, SerializationCause cause, SerializationIntrinsicType intrinsicType = SerializationIntrinsicType.None )
        {
            if ( intrinsicType == SerializationIntrinsicType.None )
            {
                intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( type );
            }

            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.None:
                case SerializationIntrinsicType.Boolean:
                case SerializationIntrinsicType.Char:
                case SerializationIntrinsicType.SByte:
                case SerializationIntrinsicType.Byte:
                case SerializationIntrinsicType.Int16:
                case SerializationIntrinsicType.UInt16:
                case SerializationIntrinsicType.Int32:
                case SerializationIntrinsicType.UInt32:
                case SerializationIntrinsicType.Int64:
                case SerializationIntrinsicType.UInt64:
                case SerializationIntrinsicType.Single:
                case SerializationIntrinsicType.Double:
                case SerializationIntrinsicType.String:
                case SerializationIntrinsicType.ObjRef:
                case SerializationIntrinsicType.DottedString:
                case SerializationIntrinsicType.Type:
                    this.binaryWriter.WriteByte( (byte)intrinsicType );
                    break;

                case SerializationIntrinsicType.Enum:
                    this.binaryWriter.WriteByte( (byte)intrinsicType );
                    //if ( this.formatter.MetadataEmitter == null )
                    //{
                        this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.Default);
                        this.WriteTypeName(type);
                    //}
                    //else
                    //{
                    //    // Since we have a MetadataEmitter, write the index of the metadata item.
                    //    this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.MetadataIndex);
                    //    this.binaryWriter.WriteCompressedInteger( this.GetMetadataIndex( type, cause ) );
                    //}
                    
                    break;

                case SerializationIntrinsicType.Array:
                    this.binaryWriter.WriteByte( (byte)intrinsicType );
                    this.binaryWriter.WriteCompressedInteger( type.GetArrayRank() );
                    this.WriteType( type.GetElementType(), cause );
                    break;

                case SerializationIntrinsicType.Struct:
                case SerializationIntrinsicType.Class:
                    {
                        
                            // We don't have a MetadataEmitter, so write the type signature explicitly.
                            var genericTypeDefinition = type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition() : type;
                            this.binaryWriter.WriteByte( (byte) intrinsicType );

                        
                            // TODO:Remove
                            // if ( this.formatter.MetadataEmitter == null )
                            // {
                                
                                if ( type.IsGenericType && !type.IsGenericTypeDefinition )
                                {
                                    this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.Generic);
                                this.WriteTypeName(genericTypeDefinition);


                                    var genericTypeArguments = type.GetGenericArguments();

                                    this.binaryWriter.WriteCompressedInteger( genericTypeArguments.Length );
                                    foreach ( var genericTypeArgument in genericTypeArguments )
                                    {
                                        this.WriteType( genericTypeArgument, cause );
                                    }
                                }
                                else
                                {
                                    this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.Default);
                                this.WriteTypeName(genericTypeDefinition);

                                }

                            // TODO: Remove
                            // }
                            // else
                            // {
                            //      // Since we have a MetadataEmitter, write the index of the metadata item.
                            //     Type surrogateType = this.GetSurrogateType( type );
                            //     this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.MetadataIndex);
                            //     this.binaryWriter.WriteCompressedInteger( this.GetMetadataIndex( surrogateType, cause ) );
                            // }
                    }
                    break;

                case SerializationIntrinsicType.GenericTypeParameter:
                    this.WriteGenericTypeParameter( type, cause );
                    break;

                case SerializationIntrinsicType.GenericMethodParameter:
                    this.WriteGenericMethodParameter( type, cause );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        //private int GetMetadataIndex( Type metadata, SerializationCause cause )
        //{
        //    try
        //    {
        //        return this.formatter.MetadataEmitter.GetMetadataIndex( metadata );
        //    }
        //    catch ( Exception exception )
        //    {
        //        throw MetaSerializationException.CreateWithCause( "Serialization", metadata, exception, cause );
        //    }
        //}

        private void WriteTypeName( Type type )
        {
            AssemblyTypeName assemblyTypeName;

            if ( type.IsGenericType && !type.IsGenericTypeDefinition )
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if ( !this.typeNameCache.TryGetValue( type, out assemblyTypeName ) )
            {
                var surrogateType = this.GetSurrogateType( type );

                string typeName;
                string assemblyName;
                this.formatter.Binder.BindToName( surrogateType, out typeName, out assemblyName );
                assemblyTypeName = new AssemblyTypeName( typeName, assemblyName );

                this.typeNameCache.Add( type, assemblyTypeName );
            }

            this.WriteTypeName( assemblyTypeName );
        }

        private Type GetSurrogateType( Type type )
        {
            Type surrogateType;
            if ( !this.surrogateTypesCache.TryGetValue( type, out surrogateType ) )
            {
                surrogateType = this.formatter.SerializerProvider.GetSurrogateType( type ) ?? type;
                this.surrogateTypesCache.Add( type, surrogateType );
            }

            return surrogateType;

        }

        private void WriteTypeName( AssemblyTypeName type )
        {
            this.binaryWriter.WriteDottedString( type.TypeName );
            this.binaryWriter.WriteString( type.AssemblyName );
        }

        private void WriteMethod( MethodBase method, SerializationCause cause )
        {
            this.WriteType( method.DeclaringType, cause );
            this.binaryWriter.WriteString( method.Name );
            this.binaryWriter.WriteString( method.ToString() );
        }

        private void WriteConstructionData( ObjectInfo objectInfo, SerializationCause cause )
        {
            if ( objectInfo.ConstructionDataWritten )
            {
                throw new InvalidOperationException();
            }

            objectInfo.ConstructionDataWritten = true;

            var objectType = objectInfo.Object.GetType();
            var intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( objectType );

            this.WriteType( objectType, cause, intrinsicType );

            var array = objectInfo.Object as Array;

            if (objectType.IsArray)
            {
                for (var i = 0; i < objectType.GetArrayRank(); i++)
                {
                    this.binaryWriter.WriteCompressedInteger( array.GetLength( i ) );
                    this.binaryWriter.WriteCompressedInteger( array.GetLowerBound( i ) );
                }
            }
            else
            {
                this.WriteArguments( objectInfo.ConstructorArguments, true, cause, objectType );
                // we write construction objects inline because they can be needed during construction
            }
        }

        private void WriteInitializationData( ObjectInfo objectInfo, SerializationCause cause )
        {
            if ( !objectInfo.ConstructionDataWritten || objectInfo.InitializationArgumentsWritten )
            {
                throw new InvalidOperationException();
            }

            objectInfo.InitializationArgumentsWritten = true;

            var type = objectInfo.Object.GetType();
            var intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( type );

            var array = objectInfo.Object as Array;

            if ( type.IsArray )
            {
                var indices = new int[array.Rank];
                this.WriteArrayElements( array, array.GetType().GetElementType(), indices, 0, cause );
            }
            else
            {
                if ( intrinsicType == SerializationIntrinsicType.Class )
                {
                    this.WriteArguments( objectInfo.InitializationArguments, false, cause, type );
                }
            }
        }

        private void WriteTypedValue( object value, bool writeInitializationDataInline, SerializationCause cause )
        {
            var type = value.GetType();
            var intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( type, true );
            this.WriteType( type, cause, intrinsicType );

            this.WriteValue( value, intrinsicType, writeInitializationDataInline, cause );
        }

        private void WriteValue( object value, SerializationIntrinsicType intrinsicType, bool writeInitializationDataInline, SerializationCause cause )
        {
            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.Byte:
                    this.binaryWriter.WriteByte( (byte)value );
                    break;

                case SerializationIntrinsicType.Int16:
                    this.binaryWriter.WriteCompressedInteger( (short)value );
                    break;

                case SerializationIntrinsicType.UInt16:
                    this.binaryWriter.WriteCompressedInteger( (ushort)value );
                    break;

                case SerializationIntrinsicType.Int32:
                    this.binaryWriter.WriteCompressedInteger( (int)value );
                    break;

                case SerializationIntrinsicType.UInt32:
                    this.binaryWriter.WriteCompressedInteger( (uint)value );
                    break;

                case SerializationIntrinsicType.Int64:
                    this.binaryWriter.WriteCompressedInteger( (long)value );
                    break;

                case SerializationIntrinsicType.UInt64:
                    this.binaryWriter.WriteCompressedInteger( (ulong)value );
                    break;

                case SerializationIntrinsicType.Single:
                    this.binaryWriter.WriteSingle( (float)value );
                    break;

                case SerializationIntrinsicType.Double:
                    this.binaryWriter.WriteDouble( (double)value );
                    break;

                case SerializationIntrinsicType.String:
                    this.binaryWriter.WriteString( (string)value );
                    break;

                case SerializationIntrinsicType.DottedString:
                    this.binaryWriter.WriteDottedString( (DottedString)value );
                    break;

                case SerializationIntrinsicType.Char:
                    this.binaryWriter.WriteCompressedInteger( (char)value );
                    break;

                case SerializationIntrinsicType.Boolean:
                    this.binaryWriter.WriteByte( (byte)(((bool)value) ? 1 : 0) );
                    break;

                case SerializationIntrinsicType.SByte:
                    this.binaryWriter.WriteSByte( (sbyte)value );
                    break;

                case SerializationIntrinsicType.Struct:
                    this.WriteStruct( value, cause );
                    break;

                case SerializationIntrinsicType.ObjRef:
                    this.WriteObjectReference( value, writeInitializationDataInline, cause );
                    break;

                case SerializationIntrinsicType.Type:
                    this.WriteType( (Type)value, cause );
                    break;

                case SerializationIntrinsicType.Enum:
                    this.binaryWriter.WriteCompressedInteger( Convert.ToInt64( value, CultureInfo.InvariantCulture  ) );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(intrinsicType));
            }
        }

        private void WriteGenericTypeParameter( Type type, SerializationCause cause )
        {
            this.binaryWriter.WriteByte( (byte)SerializationIntrinsicType.GenericTypeParameter );
            this.WriteType( type.DeclaringType, cause );
            this.binaryWriter.WriteCompressedInteger( type.GenericParameterPosition );
        }

        private void WriteGenericMethodParameter( Type type, SerializationCause cause )
        {
            this.binaryWriter.WriteByte( (byte)SerializationIntrinsicType.GenericMethodParameter );
            this.WriteMethod( type.DeclaringMethod, cause );
            this.binaryWriter.WriteCompressedInteger( type.GenericParameterPosition );
        }

        private void WriteObjectReference( object value, bool writeInitializationDataInline, SerializationCause cause )
        {
            var objectInfo = this.GetObjectInfo( value, cause );

            this.binaryWriter.WriteCompressedInteger( objectInfo.ObjectId );

            if ( !objectInfo.ConstructionDataWritten )
            {
                // we enqueue the object first to maintain order of objects. During writing construction data objects can be added to queue
                if ( !writeInitializationDataInline )
                {
                    this.serializationQueue.Enqueue( new SerializationQueueItem<object>(  objectInfo.Object, cause ) );
                }

                this.WriteConstructionData( objectInfo, cause );

                if ( writeInitializationDataInline )
                {
                    this.WriteInitializationData( objectInfo, cause );
                }
            }
        }

        private void WriteStruct( object value, SerializationCause cause )
        {
            var type = value.GetType();
            var serializer = this.formatter.SerializerProvider.GetSerializer( type );
            var arguments = new Arguments( this.formatter );

            this.TrySerialize( serializer, value, arguments, null, cause );

            // structs have one phase serializers so we have to write initialization data inline
            this.WriteArguments( arguments, true, cause, type );
        }

        private void WriteArguments( Arguments arguments, bool writeInitializationArgumentsInline, SerializationCause cause, Type owningType )
        {
            var count = 0;
            
            if ( arguments.Values.Count > 0 )
            {
                foreach ( var argument in arguments.Values )
                {
                    if ( argument.Value != null )
                    {
                        count++;
                    }
                }
            }

            this.binaryWriter.WriteCompressedInteger( count );

            if ( count > 0 )
            {
                foreach ( var argument in arguments.Values )
                {
                    if ( argument.Value == null )
                    {
                        continue;
                    }

                    this.binaryWriter.WriteDottedString( argument.Key );
                    var newCause = this.shouldReportExceptionCause
                        ? SerializationCause.WithTypedValue( cause, argument.Key, owningType )
                        : cause;
                    this.WriteTypedValue( argument.Value, writeInitializationArgumentsInline,
                                          newCause );
                }
            }
        }

        private void WriteObject( object obj, SerializationCause cause )
        {
            
            if ( obj == null )
            {
                this.binaryWriter.WriteByte( (byte)SerializationIntrinsicType.None );
                return;
            }

            var intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( obj.GetType() );

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
                case SerializationIntrinsicType.String:
                case SerializationIntrinsicType.DottedString:
                case SerializationIntrinsicType.Char:
                case SerializationIntrinsicType.Boolean:
                case SerializationIntrinsicType.SByte:
                case SerializationIntrinsicType.Struct:
                case SerializationIntrinsicType.Enum:
                case SerializationIntrinsicType.Type:
                    this.WriteTypedValue( obj, false, cause );
                    break;

                case SerializationIntrinsicType.Class:
                case SerializationIntrinsicType.Array:
                    var objectInfo = this.GetObjectInfo( obj, cause );

                    if ( !objectInfo.ConstructionDataWritten )
                    {
                        // we enqueue the object first to maintain order of objects. During writing construction data objects can be added to queue
                        this.serializationQueue.Enqueue( new SerializationQueueItem<object>(  obj, cause ) );
                        this.WriteConstructionData( objectInfo, cause );
                    }
                    else if ( objectInfo.ConstructionDataWritten && !objectInfo.InitializationArgumentsWritten ) // we can check this only in Array|Class because only these items get added on the queue from serialization code
                    {
                        this.WriteInitializationData( objectInfo, cause );
                    }

                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void WriteArrayElements( Array array, Type elementType, int[] indices, int currentDimension, SerializationCause cause )
        {
            var length = array.GetLength( currentDimension );
            var lowerBound = array.GetLowerBound( currentDimension );

            if ( currentDimension + 1 < indices.Length )
            {
                for ( var i = 0; i < length; i++ )
                {
                    indices[currentDimension] = i;
                    this.WriteArrayElements( array, elementType, indices, currentDimension + 1, cause );
                }
            }
            else
            {
                var elementIntrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( elementType, true );

                for ( var i = lowerBound; i < lowerBound + length; i++ )
                {
                    indices[currentDimension] = i;
                    var value = array.GetValue( indices );

                    // shouldn’t structs be written with type (inheritance is possible) 
                    var newCause = this.shouldReportExceptionCause ? SerializationCause.WithIndices( cause, i ) : cause;
                    if ( elementType.IsValueType )
                    {
                        this.WriteValue( value, elementIntrinsicType, false, newCause );
                    }
                    else if ( value == null )
                    {
                        this.binaryWriter.WriteByte( (byte)SerializationIntrinsicType.None );
                    }
                    else
                    {
                        // array can be constructed without fully deserializing its elements
                        this.WriteTypedValue( value, false, newCause );
                    }
                }
            }
        }

        private sealed class ObjectInfo
        {
            public readonly object Object;

            public readonly int ObjectId;

            public readonly Arguments ConstructorArguments;

            public readonly Arguments InitializationArguments;

            public ObjectInfo( object o, int objectId, MetaFormatter formatter )
            {
                this.InitializationArguments = new Arguments( formatter );
                this.ConstructorArguments = new Arguments( formatter );
                this.Object = o;
                this.ObjectId = objectId;
            }

            public bool ConstructionDataWritten;

            public bool InitializationArgumentsWritten;
        }

        private sealed class CanonicalComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals( object x, object y )
            {
                return ReferenceEquals( x, y );
            }

            public int GetHashCode( object obj )
            {
                return RuntimeHelpers.GetHashCode( obj );
            }
        }

        private class Arguments : IArgumentsWriter
        {
            private readonly MetaFormatter formatter;

            public readonly Dictionary<string, object> Values = new Dictionary<string, object>( StringComparer.Ordinal );

            public Arguments( MetaFormatter formatter )
            {
                this.formatter = formatter;
            }

            public void SetValue( string name, object value, string scope = null )
            {
                if ( value == null )
                {
                    return;
                }

                if ( scope != null )
                {
                    name = scope + "." + name;
                }
                this.Values[name] = value;
            }

            // TODO: Remove
            //public IMetadataEmitter MetadataEmitter { get { return this.formatter.MetadataEmitter;} }
        }
    }
}