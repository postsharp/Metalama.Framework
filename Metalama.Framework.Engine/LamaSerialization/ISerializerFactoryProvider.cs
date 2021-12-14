// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Engine.LamaSerialization
{
    /// <summary>
    /// Provides instances of the <see cref="ISerializerFactory"/> interface given the object type.
    /// </summary>
    /// <seealso cref="ISerializerDiscoverer"/>
    internal interface ISerializerFactoryProvider : IService
    {
        /// <summary>
        /// Gets the surrogate type for a given object type. 
        /// </summary>
        /// <param name="objectType">Type of the object being serialized.</param>
        /// <returns>The surrogate type for <paramref name="objectType"/>, or <c>null</c> (or <paramref name="objectType"/>) is <paramref name="objectType"/>
        /// does not require a surrogate type.</returns>
        /// <remarks>
        ///     <para>This method is called only during serialization. The surrogate type is the type whose name will be serialized; therefore, it will also
        ///             be the type that will be deserialized.</para>
        /// <para>It is <i>not</i> the responsibility of this class to call the next provider (<see cref="NextProvider"/>).</para>
        /// </remarks>
        Type? GetSurrogateType( Type objectType );

        /// <summary>
        /// Gets the instance of <see cref="ISerializerFactory"/>.
        /// </summary>
        /// <param name="objectType">Type of object being serialized or deserialized. If a surrogate type has been specified during serialization,
        /// this parameter is set to the surrogate type during deserialization.</param>
        /// <returns>An instance of <see cref="ISerializerFactory"/> able to serialize or deserialize <paramref name="objectType"/>, or <c>null</c>
        /// if there is no known serializer factory for this object. </returns>
        /// <remarks>
        /// <para>It is <i>not</i> the responsibility of this class to call the next provider (<see cref="NextProvider"/>).</para>
        /// </remarks>
        ISerializerFactory? GetSerializerFactory( Type objectType );

        /// <summary>
        /// Gets the next provider in the chain.
        /// </summary>
        ISerializerFactoryProvider? NextProvider { get; }
    }
}