// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime.Serialization;
using System;
using System.Collections;
using System.IO;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class SerializationTestsBase
    {
        public T TestSerialization<T>( T instance, Func<T, T, bool> assert = null )
        {
            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( instance, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (T) formatter.Deserialize( memoryStream );

            var orgCol = instance as ICollection;
            var newCol = deserializedObject as ICollection;

            if ( assert != null )
            {
                assert( instance, deserializedObject );
            }
            else if ( orgCol != null )
            {
                Assert.Equal( orgCol, newCol );
            }
            else
            {
                Assert.Equal( instance, deserializedObject );
            }

            return deserializedObject;
        }

        public T SerializeDeserialize<T>( T value )
        {
            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();

            formatter.Serialize( value, memoryStream );

            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserialized = (T) formatter.Deserialize( memoryStream );

            return deserialized;
        }
    }
}