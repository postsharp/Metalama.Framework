// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Serialization
{
    /// <summary>
    /// Custom attribute that, when applied to a type, specifies its serializer for use by the <see cref="ILamaSerializable"/>.
    /// </summary>
    /// <remarks>
    ///     <para>The use of the <see cref="SerializerAttribute"/> is optional if the serializer of a type is a nested class of that class named <c>Serializer</c>.</para>
    ///     <para>Windows Phone 7.0 does not allow to assign generic types to parameters of custom attributes.</para>
    /// </remarks>
    /// <seealso cref="ImportSerializerAttribute"/>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct )]
    public sealed class SerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerAttribute"/> class.
        /// </summary>
        /// <param name="serializerType">Serializer type. This type must implement <see cref="ISerializer"/> or <see cref="ISerializerFactory"/>,
        /// and must have a public default constructor. If <paramref name="serializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as the target type, and have a compatible set of constraints.</param>
        public SerializerAttribute( Type serializerType )
        {
            this.SerializerType = serializerType;
        }

        /// <summary>
        /// Gets the serializer type.
        /// </summary>
        /// <remarks>
        /// This type must implement <see cref="ISerializer"/> or <see cref="ISerializerFactory"/>,
        /// and must have a public default constructor. If <see cref="SerializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as the target type, and have a compatible set of constraints.
        /// </remarks>
        public Type SerializerType { get; }
    }
}