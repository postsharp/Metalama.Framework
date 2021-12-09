// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    internal class LamaFormatter
    {
        /// <summary>
        /// Gets or sets the default <see cref="LamaSerializationBinder"/> that is used by a <see cref="LamaFormatter"/> to bind types to/from type names if no
        /// <see cref="LamaSerializationBinder"/> is specified.
        /// </summary>
        public static LamaSerializationBinder? DefaultBinder { get; set; }

        /// <summary>
        /// Gets the <see cref="LamaSerializationBinder"/> used by the current <see cref="LamaFormatter"/> to bind types to/from type names.
        /// </summary>
        internal LamaSerializationBinder Binder { get; }

        internal MetaSerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LamaFormatter"/> class.
        /// </summary>
        public LamaFormatter() : this( null, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LamaFormatter"/> class.
        /// </summary>
        /// <param name="binder">A <see cref="LamaSerializationBinder"/> customizing bindings between types and type names, or <c>null</c> to use the default implementation.</param>
        /// <param name="serializerProvider">A custom implementation of <see cref="ISerializerFactoryProvider"/>, or <c>null</c> to use the default implementation.</param>
        public LamaFormatter( LamaSerializationBinder? binder, ISerializerFactoryProvider? serializerProvider )
        {
            this.Binder = binder ?? DefaultBinder ?? new LamaSerializationBinder();
            this.SerializerProvider = new MetaSerializerProvider( serializerProvider ?? SerializerFactoryProvider.BuiltIn );
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
                var serializationWriter = new SerializationWriter( stream, this, shouldReportExceptionCause: false );
                serializationWriter.Serialize( obj );
            }
            catch ( LamaSerializationException )
            {
                var serializationWriter = new SerializationWriter( Stream.Null, this, shouldReportExceptionCause: true );
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