// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.LamaSerialization
{
    /// <summary>
    /// Provides instances of the <see cref="ISerializerFactory"/> interface for object types that have been previously registered
    /// using <see cref="AddSerializer"/>.
    /// </summary>
    internal class SerializerFactoryProvider : ISerializerFactoryProvider
    {
        private readonly Dictionary<Type, ISerializerFactory> _serializerTypes = new( 64 );

        private bool _isReadOnly;

        /// <inheritdoc />
        public ISerializerFactoryProvider? NextProvider { get; }

        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Forbids further changes in the current <see cref="SerializerFactoryProvider"/>.
        /// </summary>
        public void MakeReadOnly()
        {
            if ( this._isReadOnly )
            {
                throw new InvalidOperationException();
            }

            this._isReadOnly = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerFactoryProvider"/> class.
        /// </summary>
        /// <param name="nextProvider">The next provider in the chain, or <c>null</c> if there is none.</param>
        /// <param name="activatorProvider"></param>
        public SerializerFactoryProvider( IServiceProvider serviceProvider, ISerializerFactoryProvider nextProvider )
        {
            this.ServiceProvider = serviceProvider;
            this.NextProvider = nextProvider;
        }

        /// <summary>
        /// Maps an object type to a serializer type (using generic type parameters).
        /// </summary>
        /// <typeparam name="TObject">Type of the serialized object.</typeparam>
        /// <typeparam name="TSerializer">Type of the serializer.</typeparam>
        public void AddSerializer<TObject, TSerializer>()
            where TSerializer : ISerializer, new()
        {
            this.AddSerializer( typeof(TObject), typeof(TSerializer) );
        }

        /// <summary>
        /// Maps an object type to a serializer type.
        /// </summary>
        /// <param name="objectType">Type of the serialized object.</param>
        /// <param name="serializerType">Type of the serializer (must be derived from <see cref="ISerializer"/>).</param>
        public void AddSerializer( Type objectType, Type serializerType )
        {
            if ( this._isReadOnly )
            {
                throw new InvalidOperationException();
            }

            if ( !typeof(ISerializer).IsAssignableFrom( serializerType ) )
            {
                throw new ArgumentOutOfRangeException( nameof(serializerType), "Type '{0}' does not implement ISerializer or IGenericSerializerFactory" );
            }

            this._serializerTypes.Add( objectType, new ReflectionSerializerFactory( this.ServiceProvider, serializerType ) );
        }

        /// <inheritdoc />
        public virtual Type? GetSurrogateType( Type objectType )
        {
            return null;
        }

        /// <inheritdoc />
        public virtual ISerializerFactory? GetSerializerFactory( Type objectType )
        {
            if ( this._serializerTypes.TryGetValue( objectType, out var serializerType ) )
            {
                return serializerType;
            }
            else if ( this.NextProvider != null )
            {
                return this.NextProvider.GetSerializerFactory( objectType );
            }
            else
            {
                return null;
            }
        }
    }
}