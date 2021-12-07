// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Exposes a method <seealso cref="DiscoverSerializers"/> that allows implementations
    /// of the <seealso cref="IMetaSerializerFactoryProvider"/> interface to discover serializer types
    /// for each type being serialized.
    /// </summary>
    /// <remarks>
    /// Only implementations of <seealso cref="IMetaSerializerFactoryProvider"/> may implement this interface.
    /// </remarks>
    /// <seealso cref="IMetaSerializerFactoryProvider"/>
    internal interface IMetaSerializerDiscoverer
    {
        /// <summary>
        /// Invoked by <seealso cref="MetaFormatter"/> once for every type that needs to be serialized,
        /// before <seealso cref="IMetaSerializerFactoryProvider.GetSurrogateType"/> is invoked.
        /// </summary>
        /// <param name="objectType">Type being serialized.</param>
        void DiscoverSerializers( Type objectType );
    }
}