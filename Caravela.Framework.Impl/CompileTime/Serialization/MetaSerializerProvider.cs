// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class MetaSerializerProvider
    {
        private readonly IMetaSerializerFactoryProvider provider;
        private readonly MetaSerializerProvider next;
        private readonly Dictionary<Type, IMetaSerializer> serializers = new Dictionary<Type, IMetaSerializer>(64);
        private readonly object sync = new object();
        
        public MetaSerializerProvider(IMetaSerializerFactoryProvider provider)
        {
            this.provider = provider;
            if ( provider.NextProvider != null )
            {
                this.next = new MetaSerializerProvider( provider.NextProvider );
            }
        }

        public Type GetSurrogateType(Type objectType)
        {
            for ( IMetaSerializerFactoryProvider currentProvider = this.provider; currentProvider != null; currentProvider = currentProvider.NextProvider )
            {
                Type surrogateType = currentProvider.GetSurrogateType( objectType );
                if ( surrogateType != null ) return surrogateType;
            }

            return objectType;
        }

        private void DiscoverSerializers( Type objectType )
        {
            for ( IMetaSerializerFactoryProvider currentProvider = this.provider; currentProvider != null; currentProvider = currentProvider.NextProvider )
            {
                IMetaSerializerDiscoverer serializerDiscoverer = currentProvider as IMetaSerializerDiscoverer;
                if ( serializerDiscoverer != null )
                    serializerDiscoverer.DiscoverSerializers( objectType );
            }
        }

        public IMetaSerializer GetSerializer(Type objectType)
        {
            IMetaSerializer serializer;

            if ( !this.TryGetSerializer( objectType, out  serializer))
            {
                throw new MetaSerializationException(string.Format(CultureInfo.InvariantCulture, "Cannot find a serializer for type '{0}'.", objectType));
            }

            return serializer;
        }

        public bool TryGetSerializer( Type objectType, out IMetaSerializer serializer )
        {
            if (objectType.HasElementType)
            {
                throw new ArgumentOutOfRangeException(nameof(objectType));
            }

            lock (this.sync)
            {

                if ( this.serializers.TryGetValue( objectType, out serializer ) )
                {
                    return true;
                }

                this.DiscoverSerializers( objectType );

                IMetaSerializerFactory serializerFactory = this.provider.GetSerializerFactory( objectType );

                if ( serializerFactory == null )
                {
                    if ( objectType.IsGenericType )
                    {
                        serializerFactory = this.provider.GetSerializerFactory( objectType.GetGenericTypeDefinition() );

                    }

                    if ( serializerFactory == null )
                    {
                        if ( this.next != null )
                        {
                            return this.next.TryGetSerializer( objectType, out serializer );
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                
                serializer = serializerFactory.CreateSerializer(objectType);

                
                this.serializers.Add( objectType, serializer );

                return true;
            }
        }


    }
}
