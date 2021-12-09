// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public enum Fail
    {
        None,
        Write,
        Read
    }

    public class SerializationExceptionTests
    {
        [Fact]
        public void TestWriteException()
        {
            var references = new[]
            {
                new ReferenceToChildren { Children = { new Child { Fail = Fail.None }, new Child { Fail = Fail.None } } },
                new ReferenceToChildren { Children = { new Child { Fail = Fail.None }, new Child { Fail = Fail.Write } } }
            };

            var formatter = new MetaFormatter( null, MetaSerializerFactoryProvider.BuiltIn );
            var memoryStream = new MemoryStream();

            try
            {
                formatter.Serialize( references, memoryStream );
            }
            catch ( MetaSerializationException ex )
            {
                Assert.Contains( "Child", ex.Message, StringComparison.Ordinal );
                Assert.Contains( "ReferenceToChildren[]::root[1].ReferenceToChildren::Children", ex.Message, StringComparison.Ordinal );
                Assert.EndsWith( "[1]", ex.Message, StringComparison.Ordinal ); // Second child fails.
            }
        }

        [Fact]
        public void TestReadException()
        {
            var references = new[]
            {
                new ReferenceToChildren { Children = { new Child { Fail = Fail.None }, new Child { Fail = Fail.None } } },
                new ReferenceToChildren { Children = { new Child { Fail = Fail.None }, new Child { Fail = Fail.Read } } }
            };

            var formatter = new MetaFormatter( null, MetaSerializerFactoryProvider.BuiltIn );
            var memoryStream = new MemoryStream();
            formatter.Serialize( references, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );

            try
            {
                formatter.Deserialize( memoryStream );
            }
            catch ( MetaSerializationException ex )
            {
                Assert.Contains( "Child", ex.Message, StringComparison.Ordinal );
                Assert.Contains( "ReferenceToChildren[]::root[1].ReferenceToChildren::Children", ex.Message, StringComparison.Ordinal );
                Assert.EndsWith( "[1]", ex.Message, StringComparison.Ordinal ); // Second child fails.
            }
        }

        [Fact]
        public void TestFormatterSerializeFail()
        {
            var formatter = new MetaFormatter();
            Child.NSerialized = 0;

            try
            {
                formatter.Serialize( new Child { Fail = Fail.Write }, Stream.Null );
            }
            catch ( MetaSerializationException ex )
            {
                Assert.EndsWith( "Child::root", ex.Message, StringComparison.Ordinal );
                Assert.Equal( 2, Child.NSerialized );
            }
        }

        [Fact]
        public void TestFormatterSerializeSuccess()
        {
            var formatter = new MetaFormatter();
            Child.NSerialized = 0;

            formatter.Serialize( new Child { Fail = Fail.None }, Stream.Null );
            Assert.Equal( 1, Child.NSerialized );
        }

        [Fact]
        public void TestFormatterDeserializeFail()
        {
            var formatter = new MetaFormatter();
            var stream = new SeekCountingMemoryStream();
            formatter.Serialize( new Child { Fail = Fail.Read }, stream );
            stream.Seek( 0, SeekOrigin.Begin );

            try
            {
                formatter.Deserialize( stream );
            }
            catch ( MetaSerializationException ex )
            {
                Assert.EndsWith( "Child::root", ex.Message, StringComparison.Ordinal );

                // One seek to deserialize, another one happens inside to restart the process.
                Assert.Equal( 2, stream.SeekCount );
            }
        }

        [Fact]
        public void TestFormatterDeserializeSuccess()
        {
            var formatter = new MetaFormatter();
            var stream = new SeekCountingMemoryStream();
            formatter.Serialize( new Child { Fail = Fail.None }, stream );
            stream.Seek( 0, SeekOrigin.Begin );

            formatter.Deserialize( stream );

            // Just one seek to deserialize.
            Assert.Equal( 1, stream.SeekCount );
        }

        private class SeekCountingMemoryStream : MemoryStream
        {
            public int SeekCount { get; private set; }

            public override long Seek( long offset, SeekOrigin loc )
            {
                this.SeekCount++;

                return base.Seek( offset, loc );
            }
        }

        [MetaSerializer( typeof(Serializer) )]
        public class Base
        {
#pragma warning disable SA1401 // Fields should be private
            public Fail Fail = Fail.None;
#pragma warning restore SA1401 // Fields should be private

            public class Serializer : ReferenceTypeSerializer<Base>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new Base();
                }

                public override void SerializeObject( Base obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    if ( obj.Fail == Fail.Write )
                    {
                        throw new MetaSerializationException();
                    }

                    initializationArguments.SetValue( "Fail", obj.Fail );
                }

                public override void DeserializeFields( Base obj, IArgumentsReader initializationArguments )
                {
                    obj.Fail = initializationArguments.GetValue<Fail>( "Fail" );

                    if ( obj.Fail == Fail.Read )
                    {
                        throw new MetaSerializationException();
                    }
                }
            }
        }

        [MetaSerializer( typeof(Serializer) )]
        public class Child : Base, IMetaSerializationCallback
        {
            public static int NSerialized { get; set; }

            public new class Serializer : ReferenceTypeSerializer<Child>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new Child();
                }

                public override void SerializeObject( Child obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    new Base.Serializer().SerializeObject( obj, constructorArguments, initializationArguments );
                }

                public override void DeserializeFields( Child obj, IArgumentsReader initializationArguments )
                {
                    new Base.Serializer().DeserializeFields( obj, initializationArguments );
                }
            }

            public void OnDeserialized() { }

            public void OnSerializing()
            {
                NSerialized++;
            }
        }

        [MetaSerializer( typeof(Serializer) )]
        public class ReferenceToChildren
        {
#pragma warning disable SA1401 // Fields should be private
            public List<Child> Children = new();
#pragma warning restore SA1401 // Fields should be private

            public class Serializer : ReferenceTypeSerializer<ReferenceToChildren>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new ReferenceToChildren();
                }

                public override void SerializeObject( ReferenceToChildren obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    initializationArguments.SetValue( "Children", obj.Children );
                }

                public override void DeserializeFields( ReferenceToChildren obj, IArgumentsReader initializationArguments )
                {
                    obj.Children = initializationArguments.GetValue<List<Child>>( "Children" ).AssertNotNull();
                }
            }
        }
    }
}