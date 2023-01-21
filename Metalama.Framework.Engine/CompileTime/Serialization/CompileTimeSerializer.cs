﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using System.IO;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal sealed class CompileTimeSerializer
    {
        private readonly ProjectServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the <see cref="CompileTimeSerializationBinder"/> used by the current <see cref="CompileTimeSerializer"/> to bind types to/from type names.
        /// </summary>
        internal CompileTimeSerializationBinder Binder { get; }

        internal SerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializer"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private CompileTimeSerializer( ProjectServiceProvider serviceProvider ) : this( serviceProvider, new CompileTimeSerializationBinder( serviceProvider ) ) { }

        private CompileTimeSerializer( CompileTimeProject project, ProjectServiceProvider serviceProvider ) : this(
            serviceProvider,
            new CompileTimeCompileTimeSerializationBinder( serviceProvider, project ) ) { }

        public static CompileTimeSerializer CreateTestInstance( ProjectServiceProvider serviceProvider ) => new( serviceProvider );

        public static CompileTimeSerializer CreateSerializingInstance( ProjectServiceProvider serviceProvider ) => new( serviceProvider );

        public static CompileTimeSerializer CreateDeserializingInstance( ProjectServiceProvider serviceProvider )
            => new( serviceProvider.GetRequiredService<CompileTimeProject>(), serviceProvider );

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializer"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="binder">A <see cref="CompileTimeSerializationBinder"/> customizing bindings between types and type names, or <c>null</c> to use the default implementation.</param>
        private CompileTimeSerializer( ProjectServiceProvider serviceProvider, CompileTimeSerializationBinder binder )
        {
            this._serviceProvider = serviceProvider;
            this.Binder = binder;
            this.SerializerProvider = new SerializerProvider( serviceProvider.GetRequiredService<ISerializerFactoryProvider>() );
        }

        /// <summary>
        /// Serializes an object (and the complete graph whose this object is the root) into a <see cref="Stream"/>.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="stream">The stream where <paramref name="obj"/> needs to be serialized.</param>
        public void Serialize( object? obj, Stream stream )
        {
            try
            {
                var serializationWriter = new SerializationWriter( this._serviceProvider, stream, this, shouldReportExceptionCause: false );
                serializationWriter.Serialize( obj );
            }
            catch ( CompileTimeSerializationException )
            {
                var serializationWriter = new SerializationWriter( this._serviceProvider, Stream.Null, this, shouldReportExceptionCause: true );
                serializationWriter.Serialize( obj );
            }
        }

        /// <summary>
        /// Deserializes a stream.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> containing a serialized object graph.</param>
        /// <returns>The root object of the object graph serialized in <paramref name="stream"/>.</returns>
        public object? Deserialize( Stream stream )
        {
            try
            {
                var serializationReader = new SerializationReader( stream, this, shouldReportExceptionCause: false );

                return serializationReader.Deserialize();
            }
            catch ( CompileTimeSerializationException ) when ( stream.CanSeek )
            {
                stream.Seek( 0, SeekOrigin.Begin );
                var serializationReader = new SerializationReader( stream, this, shouldReportExceptionCause: true );

                return serializationReader.Deserialize();
            }
        }
    }
}