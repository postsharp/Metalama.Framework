// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using System;
using System.IO;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal class LamaFormatter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the <see cref="LamaSerializationBinder"/> used by the current <see cref="LamaFormatter"/> to bind types to/from type names.
        /// </summary>
        internal LamaSerializationBinder Binder { get; }

        internal SerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LamaFormatter"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private LamaFormatter( IServiceProvider serviceProvider ) : this( serviceProvider, new LamaSerializationBinder() ) { }

        private LamaFormatter( CompileTimeProject project, IServiceProvider serviceProvider ) : this(
            serviceProvider,
            new CompileTimeLamaSerializationBinder( project ) ) { }

        public static LamaFormatter CreateTestInstance( IServiceProvider serviceProvider ) => new( serviceProvider );

        public static LamaFormatter CreateSerializingInstance( IServiceProvider serviceProvider ) => new( serviceProvider );

        public static LamaFormatter CreateDeserializingInstance( IServiceProvider serviceProvider )
            => new( serviceProvider.GetRequiredService<CompileTimeProject>(), serviceProvider );

        /// <summary>
        /// Initializes a new instance of the <see cref="LamaFormatter"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="binder">A <see cref="LamaSerializationBinder"/> customizing bindings between types and type names, or <c>null</c> to use the default implementation.</param>
        private LamaFormatter( IServiceProvider serviceProvider, LamaSerializationBinder binder )
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
            catch ( LamaSerializationException )
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
            catch ( LamaSerializationException ) when ( stream.CanSeek )
            {
                stream.Seek( 0, SeekOrigin.Begin );
                var serializationReader = new SerializationReader( stream, this, shouldReportExceptionCause: true );

                return serializationReader.Deserialize();
            }
        }
    }
}