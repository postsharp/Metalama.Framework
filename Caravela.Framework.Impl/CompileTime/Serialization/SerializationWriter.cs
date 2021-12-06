﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal class SerializationWriter
    {
        private const int _version = 1;

        private readonly SerializationBinaryWriter _binaryWriter;

        private readonly Queue<SerializationQueueItem<object>> _serializationQueue = new Queue<SerializationQueueItem<object>>();

        private readonly MetaFormatter _formatter;
        private readonly bool _shouldReportExceptionCause;

        private readonly Dictionary<Type, AssemblyTypeName> _typeNameCache = new Dictionary<Type, AssemblyTypeName>();
        private readonly Dictionary<Type, Type> _surrogateTypesCache = new Dictionary<Type, Type>();
        private readonly Dictionary<object, ObjectInfo> _objects = new Dictionary<object, ObjectInfo>( new CanonicalComparer() );

        public SerializationWriter( Stream stream, MetaFormatter formatter, bool shouldReportExceptionCause )
        {
            this._formatter = formatter;
            this._shouldReportExceptionCause = shouldReportExceptionCause;
            this._binaryWriter = new SerializationBinaryWriter( new BinaryWriter( stream ) );
        }

        public void Serialize( object obj )
        {
            this._binaryWriter.WriteCompressedInteger( _version );

            var cause = this._shouldReportExceptionCause ? SerializationCause.WithTypedValue( null, "root", obj.GetType() ) : null;

            this._serializationQueue.Enqueue(
                new SerializationQueueItem<object>( obj, cause ) );

            while ( this._serializationQueue.Count > 0 )
            {
                var item = this._serializationQueue.Dequeue();

                this.WriteObject( item.Value, item.Cause );
            }

            this._binaryWriter.WriteByte( (byte) SerializationIntrinsicType.None );
        }

        private static void CallOnSerialization( object obj )
        {
            ISerializationCallback? callback;
            if ( (callback = obj as ISerializationCallback) != null )
            {
                callback.OnSerializing();
            }
        }

        private ObjectInfo GetObjectInfo( object obj, SerializationCause? cause )
        {
            var type = obj.GetType();

            if ( type.IsValueType )
            {
                throw new ArgumentOutOfRangeException( nameof( obj ) );
            }

            if ( !this._objects.TryGetValue( obj, out var objectInfo ) )
            {
                CallOnSerialization( obj );

                var serializer = type.IsArray ? null : this._formatter.SerializerProvider.GetSerializer( type );

                objectInfo = new ObjectInfo( obj, this._objects.Count + 1, this._formatter );

                if ( !type.IsArray )
                {
                    TrySerialize( serializer.AssertNotNull(), obj, objectInfo.ConstructorArguments, objectInfo.InitializationArguments, cause );
                }

                this._objects.Add( obj, objectInfo );
            }

            return objectInfo;
        }

        private static void TrySerialize( IMetaSerializer serializer, object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments, SerializationCause? cause )
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

        private void WriteType( Type type, SerializationCause? cause, SerializationIntrinsicType intrinsicType = SerializationIntrinsicType.None )
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
                    this._binaryWriter.WriteByte( (byte) intrinsicType );
                    break;

                case SerializationIntrinsicType.Enum:
                    this._binaryWriter.WriteByte( (byte) intrinsicType );

                    // if ( this.formatter.MetadataEmitter == null )
                    // {
                    this._binaryWriter.WriteByte( (byte) SerializationIntrinsicTypeFlags.Default );
                    this.WriteTypeName( type );

                    // }
                    // else
                    // {
                    //    // Since we have a MetadataEmitter, write the index of the metadata item.
                    //    this.binaryWriter.WriteByte((byte)SerializationIntrinsicTypeFlags.MetadataIndex);
                    //    this.binaryWriter.WriteCompressedInteger( this.GetMetadataIndex( type, cause ) );
                    // }

                    break;

                case SerializationIntrinsicType.Array:
                    this._binaryWriter.WriteByte( (byte) intrinsicType );
                    this._binaryWriter.WriteCompressedInteger( type.GetArrayRank() );
                    this.WriteType( type.GetElementType(), cause );
                    break;

                case SerializationIntrinsicType.Struct:
                case SerializationIntrinsicType.Class:
                    {

                        // We don't have a MetadataEmitter, so write the type signature explicitly.
                        var genericTypeDefinition = type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition() : type;
                        this._binaryWriter.WriteByte( (byte) intrinsicType );

                        // TODO:Remove
                        // if ( this.formatter.MetadataEmitter == null )
                        // {

                        if ( type.IsGenericType && !type.IsGenericTypeDefinition )
                        {
                            this._binaryWriter.WriteByte( (byte) SerializationIntrinsicTypeFlags.Generic );
                            this.WriteTypeName( genericTypeDefinition );

                            var genericTypeArguments = type.GetGenericArguments();

                            this._binaryWriter.WriteCompressedInteger( genericTypeArguments.Length );
                            foreach ( var genericTypeArgument in genericTypeArguments )
                            {
                                this.WriteType( genericTypeArgument, cause );
                            }
                        }
                        else
                        {
                            this._binaryWriter.WriteByte( (byte) SerializationIntrinsicTypeFlags.Default );
                            this.WriteTypeName( genericTypeDefinition );
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

                default:
                    throw new ArgumentOutOfRangeException( nameof( type ) );
            }
        }

        // private int GetMetadataIndex( Type metadata, SerializationCause cause )
        // {
        //    try
        //    {
        //        return this.formatter.MetadataEmitter.GetMetadataIndex( metadata );
        //    }
        //    catch ( Exception exception )
        //    {
        //        throw MetaSerializationException.CreateWithCause( "Serialization", metadata, exception, cause );
        //    }
        // }

        private void WriteTypeName( Type type )
        {

            if ( type.IsGenericType && !type.IsGenericTypeDefinition )
            {
                throw new ArgumentOutOfRangeException( nameof( type ) );
            }

            if ( !this._typeNameCache.TryGetValue( type, out var assemblyTypeName ) )
            {
                var surrogateType = this.GetSurrogateType( type );

                this._formatter.Binder.BindToName( surrogateType, out var typeName, out var assemblyName );
                assemblyTypeName = new AssemblyTypeName( typeName, assemblyName );

                this._typeNameCache.Add( type, assemblyTypeName );
            }

            this.WriteTypeName( assemblyTypeName );
        }

        private Type GetSurrogateType( Type type )
        {
            if ( !this._surrogateTypesCache.TryGetValue( type, out var surrogateType ) )
            {
                surrogateType = this._formatter.SerializerProvider.GetSurrogateType( type ) ?? type;
                this._surrogateTypesCache.Add( type, surrogateType );
            }

            return surrogateType;
        }

        private void WriteTypeName( AssemblyTypeName type )
        {
            this._binaryWriter.WriteDottedString( type.TypeName );
            this._binaryWriter.WriteString( type.AssemblyName );
        }

        private void WriteConstructionData( ObjectInfo objectInfo, SerializationCause? cause )
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

            if ( objectType.IsArray )
            {
                for ( var i = 0; i < objectType.GetArrayRank(); i++ )
                {
                    this._binaryWriter.WriteCompressedInteger( array!.GetLength( i ) );
                    this._binaryWriter.WriteCompressedInteger( array!.GetLowerBound( i ) );
                }
            }
            else
            {
                this.WriteArguments( objectInfo.ConstructorArguments, true, cause, objectType );

                // we write construction objects inline because they can be needed during construction
            }
        }

        private void WriteInitializationData( ObjectInfo objectInfo, SerializationCause? cause )
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
                var indices = new int[array!.Rank];
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

        private void WriteTypedValue( object value, bool writeInitializationDataInline, SerializationCause? cause )
        {
            var type = value.GetType();
            var intrinsicType = SerializationIntrinsicTypeExtensions.GetIntrinsicType( type, true );
            this.WriteType( type, cause, intrinsicType );

            this.WriteValue( value, intrinsicType, writeInitializationDataInline, cause );
        }

        private void WriteValue( object value, SerializationIntrinsicType intrinsicType, bool writeInitializationDataInline, SerializationCause? cause )
        {
            switch ( intrinsicType )
            {
                case SerializationIntrinsicType.Byte:
                    this._binaryWriter.WriteByte( (byte) value );
                    break;

                case SerializationIntrinsicType.Int16:
                    this._binaryWriter.WriteCompressedInteger( (short) value );
                    break;

                case SerializationIntrinsicType.UInt16:
                    this._binaryWriter.WriteCompressedInteger( (ushort) value );
                    break;

                case SerializationIntrinsicType.Int32:
                    this._binaryWriter.WriteCompressedInteger( (int) value );
                    break;

                case SerializationIntrinsicType.UInt32:
                    this._binaryWriter.WriteCompressedInteger( (uint) value );
                    break;

                case SerializationIntrinsicType.Int64:
                    this._binaryWriter.WriteCompressedInteger( (long) value );
                    break;

                case SerializationIntrinsicType.UInt64:
                    this._binaryWriter.WriteCompressedInteger( (ulong) value );
                    break;

                case SerializationIntrinsicType.Single:
                    this._binaryWriter.WriteSingle( (float) value );
                    break;

                case SerializationIntrinsicType.Double:
                    this._binaryWriter.WriteDouble( (double) value );
                    break;

                case SerializationIntrinsicType.String:
                    this._binaryWriter.WriteString( (string) value );
                    break;

                case SerializationIntrinsicType.DottedString:
                    this._binaryWriter.WriteDottedString( (DottedString) value );
                    break;

                case SerializationIntrinsicType.Char:
                    this._binaryWriter.WriteCompressedInteger( (char) value );
                    break;

                case SerializationIntrinsicType.Boolean:
                    this._binaryWriter.WriteByte( (byte) (((bool) value) ? 1 : 0) );
                    break;

                case SerializationIntrinsicType.SByte:
                    this._binaryWriter.WriteSByte( (sbyte) value );
                    break;

                case SerializationIntrinsicType.Struct:
                    this.WriteStruct( value, cause );
                    break;

                case SerializationIntrinsicType.ObjRef:
                    this.WriteObjectReference( value, writeInitializationDataInline, cause );
                    break;

                case SerializationIntrinsicType.Type:
                    this.WriteType( (Type) value, cause );
                    break;

                case SerializationIntrinsicType.Enum:
                    this._binaryWriter.WriteCompressedInteger( Convert.ToInt64( value, CultureInfo.InvariantCulture ) );
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof( intrinsicType ) );
            }
        }

        private void WriteGenericTypeParameter( Type type, SerializationCause? cause )
        {
            this._binaryWriter.WriteByte( (byte) SerializationIntrinsicType.GenericTypeParameter );
            this.WriteType( type.DeclaringType, cause );
            this._binaryWriter.WriteCompressedInteger( type.GenericParameterPosition );
        }

        private void WriteObjectReference( object value, bool writeInitializationDataInline, SerializationCause? cause )
        {
            var objectInfo = this.GetObjectInfo( value, cause );

            this._binaryWriter.WriteCompressedInteger( objectInfo.ObjectId );

            if ( !objectInfo.ConstructionDataWritten )
            {
                // we enqueue the object first to maintain order of objects. During writing construction data objects can be added to queue
                if ( !writeInitializationDataInline )
                {
                    this._serializationQueue.Enqueue( new SerializationQueueItem<object>( objectInfo.Object, cause ) );
                }

                this.WriteConstructionData( objectInfo, cause );

                if ( writeInitializationDataInline )
                {
                    this.WriteInitializationData( objectInfo, cause );
                }
            }
        }

        private void WriteStruct( object value, SerializationCause? cause )
        {
            var type = value.GetType();
            var serializer = this._formatter.SerializerProvider.GetSerializer( type );
            var arguments = new Arguments();

            TrySerialize( serializer, value, arguments, ThrowingArguments.Instance, cause );

            // structs have one phase serializers so we have to write initialization data inline
            this.WriteArguments( arguments, true, cause, type );
        }

        private void WriteArguments( Arguments arguments, bool writeInitializationArgumentsInline, SerializationCause? cause, Type owningType )
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

            this._binaryWriter.WriteCompressedInteger( count );

            if ( count > 0 )
            {
                foreach ( var argument in arguments.Values )
                {
                    if ( argument.Value == null )
                    {
                        continue;
                    }

                    this._binaryWriter.WriteDottedString( argument.Key );
                    var newCause = this._shouldReportExceptionCause
                        ? SerializationCause.WithTypedValue( cause, argument.Key, owningType )
                        : cause;
                    this.WriteTypedValue( argument.Value, writeInitializationArgumentsInline, newCause );
                }
            }
        }

        private void WriteObject( object? obj, SerializationCause? cause )
        {

            if ( obj == null )
            {
                this._binaryWriter.WriteByte( (byte) SerializationIntrinsicType.None );
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
                        this._serializationQueue.Enqueue( new SerializationQueueItem<object>( obj, cause ) );
                        this.WriteConstructionData( objectInfo, cause );
                    }
                    else if ( objectInfo.ConstructionDataWritten && !objectInfo.InitializationArgumentsWritten )
                    {
                        // we can check this only in Array|Class because only these items get added on the queue from serialization code
                        this.WriteInitializationData( objectInfo, cause );
                    }

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        private void WriteArrayElements( Array array, Type elementType, int[] indices, int currentDimension, SerializationCause? cause )
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
                    var newCause = this._shouldReportExceptionCause ? SerializationCause.WithIndices( cause, i ) : cause;
                    if ( elementType.IsValueType )
                    {
                        this.WriteValue( value, elementIntrinsicType, false, newCause );
                    }
                    else if ( value == null )
                    {
                        this._binaryWriter.WriteByte( (byte) SerializationIntrinsicType.None );
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
            public object Object { get; }

            public int ObjectId { get; }

            public Arguments ConstructorArguments { get; }

            public Arguments InitializationArguments { get; }

            public bool ConstructionDataWritten { get; set; }

            public bool InitializationArgumentsWritten { get; set; }

            public ObjectInfo( object o, int objectId, MetaFormatter formatter )
            {
                this.InitializationArguments = new Arguments();
                this.ConstructorArguments = new Arguments();
                this.Object = o;
                this.ObjectId = objectId;
            }
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
#pragma warning disable SA1401 // Fields should be private
            public readonly Dictionary<string, object> Values = new( StringComparer.Ordinal );
#pragma warning restore SA1401 // Fields should be private

            public void SetValue( string name, object? value, string? scope = null )
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
        }

        private class ThrowingArguments : IArgumentsWriter
        {
            public static readonly ThrowingArguments Instance = new ThrowingArguments();

            public void SetValue( string name, object? value, string? scope = null )
            {
                throw new NotSupportedException();
            }
        }
    }
}