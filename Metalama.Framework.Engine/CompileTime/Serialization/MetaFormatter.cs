// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Metalama.Framework.Impl.CompileTime.Serialization
{
    internal class MetaFormatter
    {
        /// <summary>
        /// Gets or sets the default <see cref="MetaSerializationBinder"/> that is used by a <see cref="MetaFormatter"/> to bind types to/from type names if no
        /// <see cref="MetaSerializationBinder"/> is specified.
        /// </summary>
        public static MetaSerializationBinder? DefaultBinder { get; set; }

        /// <summary>
        /// Gets the <see cref="MetaSerializationBinder"/> used by the current <see cref="MetaFormatter"/> to bind types to/from type names.
        /// </summary>
        internal MetaSerializationBinder Binder { get; }

        internal MetaSerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaFormatter"/> class.
        /// </summary>
        public MetaFormatter() : this( null, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaFormatter"/> class.
        /// </summary>
        /// <param name="binder">A <see cref="MetaSerializationBinder"/> customizing bindings between types and type names, or <c>null</c> to use the default implementation.</param>
        /// <param name="serializerProvider">A custom implementation of <see cref="IMetaSerializerFactoryProvider"/>, or <c>null</c> to use the default implementation.</param>
        public MetaFormatter( MetaSerializationBinder? binder, IMetaSerializerFactoryProvider? serializerProvider )
        {
            this.Binder = binder ?? DefaultBinder ?? new MetaSerializationBinder();
            this.SerializerProvider = new MetaSerializerProvider( serializerProvider ?? MetaSerializerFactoryProvider.BuiltIn );
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
            catch ( MetaSerializationException )
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
            catch ( MetaSerializationException ) when ( stream.CanSeek )
            {
                stream.Seek( 0, SeekOrigin.Begin );
                var serializationReader = new SerializationReader( stream, this, shouldReportExceptionCause: true );

                return serializationReader.Deserialize();
            }
        }
    }
}