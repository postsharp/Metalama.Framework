// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    internal sealed class ReflectionMetaSerializationProvider : IMetaSerializerFactoryProvider, IMetaSerializerDiscoverer
    {
        readonly Dictionary<Type, IMetaSerializerFactory> serializerTypes = new Dictionary<Type, IMetaSerializerFactory>();
        readonly Dictionary<Type,bool> inspectedTypes = new Dictionary<Type, bool>();
        readonly Dictionary<Assembly,bool> inspectedAssemblies = new Dictionary<Assembly, bool>();
        readonly object sync = new object();
        private readonly ActivatorProvider activatorProvider;

        public ReflectionMetaSerializationProvider( ActivatorProvider activatorProvider )
        {
            this.activatorProvider = activatorProvider;
        }

        public ActivatorProvider ActivatorProvider { get { return this.activatorProvider; } }

        public Type GetSurrogateType( Type objectType )
        {
            throw new NotImplementedException();
        }

        public IMetaSerializerFactory GetSerializerFactory( Type objectType )
        {
            // If we have a generic type instance, we return null and wait to be called a second time with the generic type definition.
            if (objectType.IsGenericType && !objectType.IsGenericTypeDefinition) return null;

            lock (this.sync)
            {
                this.InspectType( objectType );

                IMetaSerializerFactory serializerType;
                this.serializerTypes.TryGetValue( objectType, out serializerType );

                return serializerType;

            }
        }

        private void AddSerializer( Type objectType, Type serializerType, ActivatorProvider activatorProvider )
        {
            IMetaSerializerFactory existingSerializerType;
            if ( this.serializerTypes.TryGetValue( objectType, out existingSerializerType ) )
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if ( ReferenceEquals( existingSerializerType,serializerType ) )
                {
                    return;
                }
                else
                {
                    throw new MetaSerializationException(string.Format(CultureInfo.InvariantCulture, "Cannot assign serializer '{0}' to type '{1}' where this type is already assigned to serializer '{2}'.",
                        serializerType, objectType, existingSerializerType));
                }
            }

            this.serializerTypes.Add(objectType, new ReflectionMetaSerializerFactory(serializerType, activatorProvider));    
            
        }

        private void InspectType( Type type )
        {
            if (this.inspectedTypes.ContainsKey(type)) return;

            this.inspectedTypes.Add(type, true);

            bool hasSerializer = false;
            try
            {
                foreach (object attribute in type.GetCustomAttributes(false))
                {
                    MetaSerializerAttribute serializableAttribute = attribute as MetaSerializerAttribute;
                    if (serializableAttribute != null && serializableAttribute.SerializerType != null )
                    {
                        hasSerializer = true;
                        this.AddSerializer(type, serializableAttribute.SerializerType, this.activatorProvider);
                        continue;
                    }

                    ImportMetaSerializerAttribute importSerializerAttribute = attribute as ImportMetaSerializerAttribute;
                    if (importSerializerAttribute != null)
                    {
                        this.ProcessImport(importSerializerAttribute);
                    }
                }
            }
            catch (FormatException)
            {
                // This happens on Windows Phone 7 if the aspect has a custom attribute with a generic type as an argument.
                // In this case, we ignore the exception and look for the Serializer nested type.
            }


            // For backward compatibility, we look for a by-convention class named "Serializer".
            if ( !hasSerializer )
            {
                Type serializerType = type.GetNestedType( "Serializer", BindingFlags.Public | BindingFlags.NonPublic );
                if ( serializerType != null )
                {
                    this.AddSerializer( type, serializerType, this.activatorProvider );
                }
            }
            
            Type baseType = type.BaseType;
            if ( baseType != null )
            {
                if (baseType.IsGenericType)
                    baseType = baseType.GetGenericTypeDefinition();

                this.InspectType( baseType );
            }

            this.InspectAssembly( type.Assembly );

        }

        private void InspectAssembly( Assembly assembly )
        {
            if (this.inspectedAssemblies.ContainsKey(assembly)) return;

            this.inspectedAssemblies.Add(assembly, true);

            foreach (ImportMetaSerializerAttribute attribute in assembly.GetCustomAttributes(typeof(ImportMetaSerializerAttribute), false))
            {
                this.ProcessImport(attribute);
            }

        }

        private void ProcessImport( ImportMetaSerializerAttribute importSerializerAttribute )
        {
            if ( importSerializerAttribute.ObjectType != null && importSerializerAttribute.SerializerType != null )
                this.AddSerializer( importSerializerAttribute.ObjectType, importSerializerAttribute.SerializerType, null );
        }

        public IMetaSerializerFactoryProvider NextProvider { get { return null; } }

        public void DiscoverSerializers( Type objectType )
        {
            lock ( this.sync )
            {
                this.InspectType( objectType );
            }
        }
    }
   
}