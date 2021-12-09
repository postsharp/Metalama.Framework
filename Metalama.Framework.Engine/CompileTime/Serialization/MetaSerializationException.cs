// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    /// <summary>
    /// Exception thrown by the <see cref="MetaFormatter"/>.
    /// </summary>
#if SERIALIZABLE
    [Serializable]
#endif
    public class MetaSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSerializationException"/> class.
        /// </summary>
        public MetaSerializationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSerializationException"/> class and specifies the message.
        /// </summary>
        /// <param name="message">Message.</param>
        public MetaSerializationException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSerializationException"/> class and specifies the message and inner exception.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner exception.</param>
        public MetaSerializationException( string message, Exception inner ) : base( message, inner ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSerializationException"/> class for deserialization purposes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MetaSerializationException(
            SerializationInfo info,
            StreamingContext context ) : base( info, context ) { }

        internal static MetaSerializationException CreateWithCause( string operation, Type type, Exception innerException, SerializationCause? cause )
        {
            var causes = new List<string>();

            while ( cause != null )
            {
                causes.Add( cause.Description );
                cause = cause.Parent;
            }

            causes.Reverse();

            var message = $"{operation} of {type.Name} failed. The order of deserialization was as follows:\n" + string.Join( "", causes.ToArray() );

            return new MetaSerializationException( message, innerException );
        }
    }
}