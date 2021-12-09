// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;
using System.Globalization;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal sealed class ReflectionSerializerFactory : ISerializerFactory
    {
        public Type SerializerType { get; }

        public ReflectionSerializerFactory( Type serializerType )
        {
            this.SerializerType = serializerType;
        }

        public ISerializer CreateSerializer( Type objectType )
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

            return instance switch
            {
                ISerializer serializer => serializer,
                ISerializerFactory serializerFactory => serializerFactory.CreateSerializer( objectType ),
                _ => throw new MetaSerializationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Type {0} must implement interface ISerializer or ISerializerFactory.",
                        serializerTypeInstance ) )
            };
        }
    }
}