// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Metalama.Framework.Serialization;
using System;
using System.Globalization;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal sealed class ReflectionSerializerFactory : ISerializerFactory
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly UserCodeExecutionContext _userCodeExecutionContext;

        public Type SerializerType { get; }

        public ReflectionSerializerFactory( ProjectServiceProvider serviceProvider, Type serializerType )
        {
            this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            this._userCodeExecutionContext = new UserCodeExecutionContext( serviceProvider );
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
                    throw new LamaSerializationException(
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

            var instance = this._userCodeInvoker.Invoke( () => Activator.CreateInstance( serializerTypeInstance ), this._userCodeExecutionContext );

            return instance switch
            {
                ISerializer serializer => serializer,
                ISerializerFactory serializerFactory => serializerFactory.CreateSerializer( objectType ),
                _ => throw new LamaSerializationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Type '{0}' must implement interface ISerializer or ISerializerFactory.",
                        serializerTypeInstance ) )
            };
        }
    }
}