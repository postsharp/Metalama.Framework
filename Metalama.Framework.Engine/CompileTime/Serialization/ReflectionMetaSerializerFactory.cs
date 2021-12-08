﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;
using System.Globalization;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal sealed class ReflectionMetaSerializerFactory : IMetaSerializerFactory
    {
        public Type SerializerType { get; }

        public ReflectionMetaSerializerFactory( Type serializerType )
        {
            this.SerializerType = serializerType;
        }

        public IMetaSerializer CreateSerializer( Type objectType )
        {
            Type serializerTypeInstance;

            if ( objectType.IsGenericTypeDefinition )
            {
                throw new ArgumentOutOfRangeException( nameof(objectType) );
            }

            if ( this.SerializerType.IsGenericTypeDefinition )
            {
                if ( this.SerializerType.GetGenericArguments().Length == objectType.GetGenericArguments().Length )
                {
                    serializerTypeInstance = this.SerializerType.MakeGenericType( objectType.GetGenericArguments() );
                }
                else
                {
                    throw new MetaSerializationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The serializer type '{0}' is an open generic type, but it has a different number of generic parameters than '{1}'.",
                            this.SerializerType,
                            objectType ) );
                }
            }
            else
            {
                serializerTypeInstance = this.SerializerType;
            }

            var instance = Activator.CreateInstance( serializerTypeInstance );

            var serializer = instance as IMetaSerializer;

            if ( serializer != null )
            {
                return serializer;
            }

            var serializerFactory = instance as IMetaSerializerFactory;

            if ( serializerFactory != null )
            {
                return serializerFactory.CreateSerializer( objectType );
            }

            throw new MetaSerializationException(
                string.Format( CultureInfo.InvariantCulture, "Type {0} must implement interface ISerializer or ISerializerFactory.", serializerTypeInstance ) );
        }
    }
}