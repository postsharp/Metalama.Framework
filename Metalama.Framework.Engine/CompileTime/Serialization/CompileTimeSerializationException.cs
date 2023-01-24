// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    /// <summary>
    /// Exception thrown by the <see cref="CompileTimeSerializer"/>.
    /// </summary>
#if SERIALIZABLE
    [Serializable]
#endif
    internal sealed class CompileTimeSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializationException"/> class.
        /// </summary>
        internal CompileTimeSerializationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializationException"/> class and specifies the message.
        /// </summary>
        /// <param name="message">Message.</param>
        public CompileTimeSerializationException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializationException"/> class and specifies the message and inner exception.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner exception.</param>
        public CompileTimeSerializationException( string message, Exception inner ) : base( message, inner ) { }

        // ReSharper disable once UnusedMember.Local

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileTimeSerializationException"/> class for deserialization purposes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private CompileTimeSerializationException(
            SerializationInfo info,
            StreamingContext context ) : base( info, context ) { }

        internal static CompileTimeSerializationException CreateWithCause( string operation, Type type, Exception innerException, SerializationCause? cause )
        {
            var causes = new List<string>();

            while ( cause != null )
            {
                causes.Add( cause.Description );
                cause = cause.Parent;
            }

            causes.Reverse();

            var message = $"{operation} of {type.Name} failed. The order of deserialization was as follows:\n" + string.Join( "", causes.ToArray() );

            return new CompileTimeSerializationException( message, innerException );
        }
    }
}