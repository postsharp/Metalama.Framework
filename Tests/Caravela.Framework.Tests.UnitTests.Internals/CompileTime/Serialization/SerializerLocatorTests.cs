// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime.Serialization;
using Caravela.Framework.Serialization;
using Caravela.Framework.Tests.UnitTests.CompileTime.Serialization;
using System;
using Xunit;

#pragma warning disable SA1106 // Code should not contain empty statements

// attribute added for testing purposes
[assembly: ImportMetaSerializer( typeof( SerializerLocatorTests.TypeWoSerializer ), typeof( SerializerLocatorTests.GenericSerializedClass<>.Serializer ) )]

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{

    public class SerializerLocatorTests
    {
        private readonly IMetaSerializerFactoryProvider _customSerializerProvider;

        public SerializerLocatorTests()
        {
            Assert.NotNull( MetaSerializerFactoryProvider.BuiltIn.NextProvider );
            this._customSerializerProvider = MetaSerializerFactoryProvider.BuiltIn.NextProvider!;
        }

        [Fact]
        public void GetSerializerType_NonGenericCustomType()
        {
            var serializerType = this._customSerializerProvider.GetSerializerFactory( typeof( SerializedClass ) ).CreateSerializer( typeof( SerializedClass ) ).GetType();

            Assert.Equal( typeof( SerializedClass.Serializer ), serializerType );
        }

        [Fact]
        public void GetSerializerType_GenericCustomTypeDefinition_NotNull()
        {
            var serializerType = this._customSerializerProvider.GetSerializerFactory( typeof( GenericSerializedClass<> ) ).CreateSerializer( typeof( GenericSerializedClass<int> ) ).GetType();

            Assert.NotNull( serializerType );
        }

        [Fact]
        public void GetSerializerType_GenericCustomTypeInstance_Null()
        {
            var serializerType = this._customSerializerProvider.GetSerializerFactory( typeof( GenericSerializedClass<string> ) );

            Assert.Null( serializerType );
        }

        [Fact]
        public void GetSerializerType_AskedTwice_TheSameObject()
        {
            var serializerType1 = this._customSerializerProvider.GetSerializerFactory( typeof( GenericSerializedClass<> ) ).CreateSerializer( typeof( GenericSerializedClass<string> ) ).GetType();
            ;
            var serializerType2 = this._customSerializerProvider.GetSerializerFactory( typeof( GenericSerializedClass<> ) ).CreateSerializer( typeof( GenericSerializedClass<string> ) ).GetType();
            ;

            Assert.Same( serializerType1, serializerType2 );
        }

        [Fact]
        public void GetSerializerType_RespectsAssemblyImportAttributes()
        {
            var assemblyImportedSerializerType = this._customSerializerProvider.GetSerializerFactory( typeof( TypeWoSerializer ) ).CreateSerializer( typeof( GenericSerializedClass<string> ) ).GetType();
            ; // NOTE: import is just over the namespace opening bracket
            var classAttributeDerivedSerializerType = this._customSerializerProvider.GetSerializerFactory( typeof( GenericSerializedClass<> ) ).CreateSerializer( typeof( GenericSerializedClass<string> ) ).GetType();
            ;

            Assert.NotNull( classAttributeDerivedSerializerType );
            Assert.NotNull( assemblyImportedSerializerType );
            Assert.Same( classAttributeDerivedSerializerType, assemblyImportedSerializerType );
        }

        [Fact]
        public void GetSerializerType_HasManySerializers_Throws()
        {
            Assert.Throws<MetaSerializationException>( () => this._customSerializerProvider.GetSerializerFactory( typeof( TypeWithManySerializers ) ) );
        }

        [MetaSerializer( typeof( Serializer ) )]
        public class SerializedClass
        {
            public class Serializer : ReferenceTypeSerializer<SerializedClass>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new SerializedClass();
                }

                public override void SerializeObject( SerializedClass obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                }

                public override void DeserializeFields( SerializedClass obj, IArgumentsReader initializationArguments )
                {
                }
            }
        }

        [MetaSerializer( typeof( GenericSerializedClass<>.Serializer ) )]
        public class GenericSerializedClass<T>
        {
            public class Serializer : ReferenceTypeSerializer<GenericSerializedClass<T>>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new GenericSerializedClass<T>();
                }

                public override void SerializeObject( GenericSerializedClass<T> obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                }

                public override void DeserializeFields( GenericSerializedClass<T> obj, IArgumentsReader initializationArguments )
                {
                }
            }
        }

        internal class TypeWoSerializer
        {
        }

        [MetaSerializer( typeof( Serializer ) )]
        [ImportMetaSerializer( typeof( TypeWithManySerializers ), typeof( SecondSerializer ) )]
        public class TypeWithManySerializers
        {
            public class Serializer : ReferenceTypeSerializer<TypeWithManySerializers>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new TypeWithManySerializers();
                }

                public override void SerializeObject( TypeWithManySerializers obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                }

                public override void DeserializeFields( TypeWithManySerializers obj, IArgumentsReader initializationArguments )
                {
                }
            }

            public class SecondSerializer : ReferenceTypeSerializer<TypeWithManySerializers>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new TypeWithManySerializers();
                }

                public override void SerializeObject( TypeWithManySerializers obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                }

                public override void DeserializeFields( TypeWithManySerializers obj, IArgumentsReader initializationArguments )
                {
                }
            }
        }
    }
}