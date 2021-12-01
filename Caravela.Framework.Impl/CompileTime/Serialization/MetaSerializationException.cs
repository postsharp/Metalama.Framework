﻿// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Exception thrown by the <see cref="MetaFormatter"/>.
    /// </summary>
#if SERIALIZABLE
    [Serializable]
#endif
    public class MetaSerializationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new <see cref="MetaSerializationException"/>.
        /// </summary>
        public MetaSerializationException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MetaSerializationException"/> and specifies the message.
        /// </summary>
        /// <param name="message">Message.</param>
        public MetaSerializationException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MetaSerializationException"/> and specifies the message and inner exception.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="inner">Inner exception.</param>
        public MetaSerializationException( string message, Exception inner ) : base( message, inner )
        {
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MetaSerializationException(
            SerializationInfo info,
            StreamingContext context ) : base( info, context )
        {
        }
        
        internal static MetaSerializationException CreateWithCause( string operation, Type type, Exception innerException, SerializationCause cause )
        {
            List<string> causes = new List<string>();
            while ( cause != null )
            {
                causes.Add(cause.Description);
                cause = cause.Parent;
            }

            causes.Reverse();

            string message = $"{operation} of {type.Name} failed. The order of deserialization was as follows:\n" + string.Join( "", causes.ToArray() );
            
            return new MetaSerializationException( message, innerException );
        }
    }
}