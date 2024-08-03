// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Services;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization
{
    public abstract partial class SerializationTestsBase : UnitTestClass
    {
        protected ProjectServiceProvider ServiceProvider { get; }

        protected SerializationTestsBase()
        {
            MetalamaEngineModuleInitializer.EnsureInitialized();

            var globalServiceProvider = ServiceProvider<IGlobalService>.Empty;
            globalServiceProvider = globalServiceProvider.WithService( new UserCodeInvoker( globalServiceProvider ) );
            var serviceProvider = ServiceProvider<IProjectService>.Empty.WithNextProvider( globalServiceProvider );
            serviceProvider = serviceProvider.WithService( new BuiltInSerializerFactoryProvider( serviceProvider ) );
            this.ServiceProvider = serviceProvider;
        }

        protected override void ConfigureServices( IAdditionalServiceCollection services )
        {
            base.ConfigureServices( services );
            services.AddProjectService( new SyntaxGenerationOptions( CodeFormattingOptions.Formatted ) );
        }

        protected override TestContext CreateTestContextCore( TestContextOptions contextOptions, IAdditionalServiceCollection services )
            => new SerializationTestContext( contextOptions, services );

        protected new SerializationTestContext CreateTestContext() => (SerializationTestContext) base.CreateTestContext();

        protected SerializationTestContext CreateTestContext( string code )
            => (SerializationTestContext) base.CreateTestContext( new SerializationTestContextOptions { Code = code } );

        protected T? TestSerialization<T>( T? instance, Func<T?, T?, bool>? assert = null )
        {
            using var testContext = this.CreateTestContext();
            var memoryStream = new MemoryStream();
            testContext.Serializer.Serialize( instance, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (T?) testContext.Serializer.Deserialize( memoryStream );

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

        protected T SerializeDeserialize<T>( T value )
        {
            using var testContext = this.CreateTestContext();

            return SerializeDeserialize( value, testContext );
        }

        protected static T SerializeDeserialize<T>( T value, SerializationTestContext testContext )
        {
            var memoryStream = new MemoryStream();

            testContext.Serializer.Serialize( value!, memoryStream );

            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserialized = (T) testContext.Serializer.Deserialize( memoryStream ).AssertNotNull();

            return deserialized;
        }
    }
}