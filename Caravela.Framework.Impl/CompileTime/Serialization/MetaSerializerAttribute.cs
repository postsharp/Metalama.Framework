// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Custom attribute that, when applied to a type, specifies its serializer for use by the <see cref="MetaFormatter"/>.
    /// </summary>
    /// <remarks>
    ///     <para>The use of the <see cref="MetaSerializerAttribute"/> is optional if the serializer of a type is a nested class of that class named <c>Serializer</c>.</para>
    ///     <para>Windows Phone 7.0 does not allow to assign generic types to parameters of custom attributes.</para>
    /// </remarks>
    /// <seealso cref="ImportMetaSerializerAttribute"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class MetaSerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="MetaSerializerAttribute"/>.
        /// </summary>
        /// <param name="serializerType">Serializer type. This type must implement <see cref="IMetaSerializer"/> or <see cref="IMetaSerializerFactory"/>,
        /// and must have a public default constructor. If <paramref name="serializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as the target type, and have a compatible set of constraints.</param>
        public MetaSerializerAttribute(Type serializerType)
        {
            this.SerializerType = serializerType;
        }

        /// <summary>
        /// Gets the serializer type.
        /// </summary>
        /// <remarks>
        /// This type must implement <see cref="IMetaSerializer"/> or <see cref="IMetaSerializerFactory"/>,
        /// and must have a public default constructor. If <see cref="SerializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as the target type, and have a compatible set of constraints.
        /// </remarks>
        public Type SerializerType { get; private set; }
        
    }
}
