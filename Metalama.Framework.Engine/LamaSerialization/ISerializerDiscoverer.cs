// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.LamaSerialization
{
    /// <summary>
    /// Exposes a method <seealso cref="DiscoverSerializers"/> that allows implementations
    /// of the <seealso cref="ISerializerFactoryProvider"/> interface to discover serializer types
    /// for each type being serialized.
    /// </summary>
    /// <remarks>
    /// Only implementations of <seealso cref="ISerializerFactoryProvider"/> may implement this interface.
    /// </remarks>
    /// <seealso cref="ISerializerFactoryProvider"/>
    internal interface ISerializerDiscoverer
    {
        /// <summary>
        /// Invoked by <seealso cref="LamaFormatter"/> once for every type that needs to be serialized,
        /// before <seealso cref="ISerializerFactoryProvider.GetSurrogateType"/> is invoked.
        /// </summary>
        /// <param name="objectType">Type being serialized.</param>
        void DiscoverSerializers( Type objectType );
    }
}