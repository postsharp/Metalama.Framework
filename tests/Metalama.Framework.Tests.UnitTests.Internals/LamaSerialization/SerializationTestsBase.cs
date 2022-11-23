// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Services;
using System;
using System.Collections;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization
{
    public abstract class SerializationTestsBase
    {
        protected ProjectServiceProvider ServiceProvider { get; }

        public SerializationTestsBase()
        {
            var globalServiceProvider = ServiceProvider<IGlobalService>.Empty;
            globalServiceProvider = globalServiceProvider.WithService( new UserCodeInvoker( globalServiceProvider ) );
            var serviceProvider = ServiceProvider<IProjectService>.Empty.WithNextProvider( globalServiceProvider );
            serviceProvider = serviceProvider.WithService( new BuiltInSerializerFactoryProvider( serviceProvider ) );
            this.ServiceProvider = serviceProvider;
        }

        public T? TestSerialization<T>( T? instance, Func<T?, T?, bool>? assert = null )
        {
            var formatter = LamaFormatter.CreateTestInstance( this.ServiceProvider );
            var memoryStream = new MemoryStream();
            formatter.Serialize( instance, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (T?) formatter.Deserialize( memoryStream );

            var newCol = deserializedObject as ICollection;

            if ( assert != null )
            {
                assert( instance, deserializedObject );
            }
            else if ( instance is ICollection orgCol )
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
            var formatter = LamaFormatter.CreateTestInstance( this.ServiceProvider );
            var memoryStream = new MemoryStream();

            formatter.Serialize( value!, memoryStream );

            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserialized = (T) formatter.Deserialize( memoryStream ).AssertNotNull();

            return deserialized;
        }
    }
}