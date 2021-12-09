// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal sealed class ReflectionMetaSerializationProvider : IMetaSerializerFactoryProvider, IMetaSerializerDiscoverer
    {
        private readonly Dictionary<Type, IMetaSerializerFactory> _serializerTypes = new();
        private readonly Dictionary<Type, bool> _inspectedTypes = new();
        private readonly Dictionary<Assembly, bool> _inspectedAssemblies = new();
        private readonly object _sync = new();

        public Type GetSurrogateType( Type objectType )
        {
            throw new NotImplementedException();
        }

        public IMetaSerializerFactory? GetSerializerFactory( Type objectType )
        {
            // If we have a generic type instance, we return null and wait to be called a second time with the generic type definition.
            if ( objectType.IsGenericType && !objectType.IsGenericTypeDefinition )
            {
                return null;
            }

            lock ( this._sync )
            {
                this.InspectType( objectType );

                this._serializerTypes.TryGetValue( objectType, out var serializerType );

                return serializerType;
            }
        }

        private void AddSerializer( Type objectType, Type serializerType )
        {
            if ( this._serializerTypes.TryGetValue( objectType, out var existingSerializerType ) )
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if ( ReferenceEquals( existingSerializerType, serializerType ) )
                {
                    return;
                }
                else
                {
                    throw new MetaSerializationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Cannot assign serializer '{0}' to type '{1}' where this type is already assigned to serializer '{2}'.",
                            serializerType,
                            objectType,
                            existingSerializerType ) );
                }
            }

            this._serializerTypes.Add( objectType, new ReflectionMetaSerializerFactory( serializerType ) );
        }

        private void InspectType( Type type )
        {
            if ( this._inspectedTypes.ContainsKey( type ) )
            {
                return;
            }

            this._inspectedTypes.Add( type, true );

            var hasSerializer = false;

            try
            {
                foreach ( var attribute in type.GetCustomAttributes( false ) )
                {
                    switch ( attribute )
                    {
                        case MetaSerializerAttribute { SerializerType: { } } serializableAttribute:
                            hasSerializer = true;
                            this.AddSerializer( type, serializableAttribute.SerializerType );

                            continue;

                        case ImportMetaSerializerAttribute importSerializerAttribute:
                            this.ProcessImport( importSerializerAttribute );

                            break;
                    }
                }
            }
            catch ( FormatException )
            {
                // This happens on Windows Phone 7 if the aspect has a custom attribute with a generic type as an argument.
                // In this case, we ignore the exception and look for the Serializer nested type.
            }

            // For backward compatibility, we look for a by-convention class named "Serializer".
            if ( !hasSerializer )
            {
                var serializerType = type.GetNestedType( "Serializer", BindingFlags.Public | BindingFlags.NonPublic );

                if ( serializerType != null )
                {
                    this.AddSerializer( type, serializerType );
                }
            }

            var baseType = type.BaseType;

            if ( baseType != null )
            {
                if ( baseType.IsGenericType )
                {
                    baseType = baseType.GetGenericTypeDefinition();
                }

                this.InspectType( baseType );
            }

            this.InspectAssembly( type.Assembly );
        }

        private void InspectAssembly( Assembly assembly )
        {
            if ( this._inspectedAssemblies.ContainsKey( assembly ) )
            {
                return;
            }

            this._inspectedAssemblies.Add( assembly, true );

            foreach ( ImportMetaSerializerAttribute attribute in assembly.GetCustomAttributes( typeof(ImportMetaSerializerAttribute), false ) )
            {
                this.ProcessImport( attribute );
            }
        }

        private void ProcessImport( ImportMetaSerializerAttribute importSerializerAttribute )
        {
            if ( importSerializerAttribute.ObjectType != null && importSerializerAttribute.SerializerType != null )
            {
                this.AddSerializer( importSerializerAttribute.ObjectType, importSerializerAttribute.SerializerType );
            }
        }

        public IMetaSerializerFactoryProvider? NextProvider => null;

        public void DiscoverSerializers( Type objectType )
        {
            lock ( this._sync )
            {
                this.InspectType( objectType );
            }
        }
    }
}