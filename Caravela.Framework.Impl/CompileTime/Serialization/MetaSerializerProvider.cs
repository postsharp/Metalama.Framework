// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class MetaSerializerProvider
    {
        private readonly IMetaSerializerFactoryProvider _provider;
        private readonly MetaSerializerProvider? _next;
        private readonly Dictionary<Type, IMetaSerializer> _serializers = new Dictionary<Type, IMetaSerializer>( 64 );
        private readonly object _sync = new object();

        public MetaSerializerProvider( IMetaSerializerFactoryProvider provider )
        {
            this._provider = provider;
            if ( provider.NextProvider != null )
            {
                this._next = new MetaSerializerProvider( provider.NextProvider );
            }
        }

        public Type GetSurrogateType( Type objectType )
        {
            for ( var currentProvider = this._provider; currentProvider != null; currentProvider = currentProvider.NextProvider )
            {
                var surrogateType = currentProvider.GetSurrogateType( objectType );
                if ( surrogateType != null )
                {
                    return surrogateType;
                }
            }

            return objectType;
        }

        private void DiscoverSerializers( Type objectType )
        {
            for ( var currentProvider = this._provider; currentProvider != null; currentProvider = currentProvider.NextProvider )
            {
                var serializerDiscoverer = currentProvider as IMetaSerializerDiscoverer;
                if ( serializerDiscoverer != null )
                {
                    serializerDiscoverer.DiscoverSerializers( objectType );
                }
            }
        }

        public IMetaSerializer GetSerializer( Type objectType )
        {

            if ( !this.TryGetSerializer( objectType, out var serializer ) )
            {
                throw new MetaSerializationException( string.Format( CultureInfo.InvariantCulture, "Cannot find a serializer for type '{0}'.", objectType ) );
            }

            return serializer;
        }

        public bool TryGetSerializer( Type objectType, out IMetaSerializer serializer )
        {
            if ( objectType.HasElementType )
            {
                throw new ArgumentOutOfRangeException( nameof( objectType ) );
            }

            lock ( this._sync )
            {

                if ( this._serializers.TryGetValue( objectType, out serializer ) )
                {
                    return true;
                }

                this.DiscoverSerializers( objectType );

                var serializerFactory = this._provider.GetSerializerFactory( objectType );

                if ( serializerFactory == null )
                {
                    if ( objectType.IsGenericType )
                    {
                        serializerFactory = this._provider.GetSerializerFactory( objectType.GetGenericTypeDefinition() );
                    }

                    if ( serializerFactory == null )
                    {
                        if ( this._next != null )
                        {
                            return this._next.TryGetSerializer( objectType, out serializer );
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                serializer = serializerFactory.CreateSerializer( objectType );

                this._serializers.Add( objectType, serializer );

                return true;
            }
        }
    }
}