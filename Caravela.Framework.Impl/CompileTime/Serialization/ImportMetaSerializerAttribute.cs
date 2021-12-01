// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Custom attribute that, when applied to a serializable type, specifies that the serializer of this type has
    /// a dependency on another serializer. The custom attribute can be applied at assembly level; it then
    /// applies to all serializable types in this assembly.
    /// </summary>
    /// <remarks>
    /// <para>This custom attribute is useful to add serializers to types of third-party assemblies.
    /// For types whose source code you can modify, it is preferable to use <see cref="IMetaSerializable"/>.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ImportMetaSerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ImportMetaSerializerAttribute"/>.
        /// </summary>
        /// <param name="objectType">Type of the object to be made serializable.</param>
        /// <param name="serializerType">Serializer type. This type must implement <see cref="IMetaSerializer"/> or <see cref="IMetaSerializerFactory"/>,
        /// and must have a public default constructor. If <paramref name="serializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as <paramref name="objectType"/>, and have a compatible set of constraints.</param>
        /// <seealso cref="MetaSerializerAttribute"/>
        public ImportMetaSerializerAttribute( Type objectType, Type serializerType )
        {
            this.ObjectType = objectType;
            this.SerializerType = serializerType;
        }

        /// <summary>
        /// Gets the type of the object to be made serializable.
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Gets the serializer type.
        /// </summary>
        /// <remarks>
        /// This type must implement <see cref="IMetaSerializer"/> or <see cref="IMetaSerializerFactory"/>,
        /// and must have a public default constructor. If <see cref="SerializerType"/> is a generic type, if must have the same number
        /// of generic type parameters as <see cref="ObjectType"/>, and have a compatible set of constraints.
        /// </remarks>
        public Type SerializerType { get; private set; }
        
    }
}
