// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Serialization
{
    /// <summary>
    /// Defines a method <see cref="CreateSerializer"/>, which creates instances of the <see cref="ISerializer"/> interface for
    /// given object types.
    /// </summary>
    internal interface ISerializerFactory
    {
        /// <summary>
        /// Creates an instance of the <see cref="ISerializer"/> interface for a given object type.
        /// </summary>
        /// <param name="objectType">Type of object being serialized or deserialized.</param>
        /// <returns>A new instance implementing the <see cref="ISerializer"/> interface.</returns>
        ISerializer CreateSerializer( Type objectType );
    }
}