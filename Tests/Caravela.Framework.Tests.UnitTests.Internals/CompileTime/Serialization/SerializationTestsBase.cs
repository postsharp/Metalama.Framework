// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

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
            MetaFormatter formatter = new MetaFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize( instance, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            T deserializedObject = (T)formatter.Deserialize( memoryStream );

            ICollection orgCol = instance as ICollection;
            ICollection newCol = deserializedObject as ICollection;

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
            MetaFormatter formatter = new MetaFormatter();
            MemoryStream memoryStream = new MemoryStream();
            
            formatter.Serialize( value, memoryStream );
            
            memoryStream.Seek( 0, SeekOrigin.Begin );
            T deserialized = (T)formatter.Deserialize( memoryStream );

            return deserialized;
        }
    }
}