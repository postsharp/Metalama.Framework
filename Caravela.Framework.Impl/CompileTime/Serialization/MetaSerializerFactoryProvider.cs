// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Caravela.Framework.Serialization;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    
    /// <summary>
    /// Provides instances of the <see cref="IMetaSerializerFactory"/> interface for object types that have been previously registered
    /// using <see cref="AddSerializer"/>.
    /// </summary>
    internal class MetaSerializerFactoryProvider : IMetaSerializerFactoryProvider 
    {
        private bool isReadOnly;
        private readonly Dictionary<Type, IMetaSerializerFactory> serializerTypes = new Dictionary<Type, IMetaSerializerFactory>( 64 );
        private readonly IMetaSerializerFactoryProvider nextProvider;
        private readonly ActivatorProvider activatorProvider;

        /// <summary>
        /// Gets the <see cref="MetaSerializerFactoryProvider"/> instance that supports built-in types.
        /// </summary>
        public static readonly MetaSerializerFactoryProvider BuiltIn = new BuiltInSerializerFactoryProvider(new ActivatorProvider());

        /// <summary>
        /// Forbids further changes in the current <see cref="MetaSerializerFactoryProvider"/>.
        /// </summary>
        public void MakeReadOnly()
        {
            if ( this.isReadOnly )
            {
                throw new InvalidOperationException();
            }
            this.isReadOnly = true;
        }

      
        /// <summary>
        /// Initializes a new <see cref="MetaSerializerFactoryProvider"/>.
        /// </summary>
        /// <param name="nextProvider">The next provider in the chain, or <c>null</c> if there is none.</param>
        /// <param name="activatorProvider"></param>
        public MetaSerializerFactoryProvider( IMetaSerializerFactoryProvider nextProvider, ActivatorProvider activatorProvider )
        {
            this.activatorProvider = activatorProvider;
            this.nextProvider = nextProvider;
        }

        /// <inheritdoc />
        public IMetaSerializerFactoryProvider NextProvider
        {
            get
            {
                return this.nextProvider;
            }
        }

        /// <summary>
        /// Maps an object type to a serializer type (using generic type parameters).
        /// </summary>
        /// <typeparam name="TObject">Type of the serialized object.</typeparam>
        /// <typeparam name="TSerializer">Type of the serializer.</typeparam>
        public void AddSerializer<TObject, TSerializer>() where TSerializer : IMetaSerializer, new()
        {
            this.AddSerializer( typeof(TObject), typeof(TSerializer) );
        }

        /// <summary>
        /// Maps an object type to a serializer type.
        /// </summary>
        /// <param name="objectType">Type of the serialized object.</param>
        /// <param name="serializerType">Type of the serializer (must be derived from <see cref="IMetaSerializer"/>).</param>
        public void AddSerializer( Type objectType, Type serializerType )
        {
            if ( this.isReadOnly )
            {
                throw new InvalidOperationException();
            }

            if ( !typeof( IMetaSerializer ).IsAssignableFrom( serializerType ) )
            {
                throw new ArgumentOutOfRangeException( nameof(serializerType), "Type '{0}' does not implement ISerializer or IGenericSerializerFactory" );
            }

            this.serializerTypes.Add( objectType, new ReflectionMetaSerializerFactory( serializerType, this.activatorProvider ) );
        }


        /// <inheritdoc />
        public virtual Type GetSurrogateType( Type objectType )
        {
            return null;
        }

        /// <inheritdoc />
        public virtual IMetaSerializerFactory GetSerializerFactory( Type objectType )
        {
            IMetaSerializerFactory serializerType;
            if (this.serializerTypes.TryGetValue(objectType, out serializerType))
                return serializerType;
            else if ( this.nextProvider != null )
                return this.nextProvider.GetSerializerFactory( objectType );
            else
                return null;
        }
    }
}