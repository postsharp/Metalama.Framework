// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Defines a method <see cref="CreateSerializer"/>, which creates instances of the <see cref="IMetaSerializer"/> interface for
    /// given object types.
    /// </summary>
    internal interface IMetaSerializerFactory
    {
        /// <summary>
        /// Creates an instance of the <see cref="IMetaSerializer"/> interface for a given object type.
        /// </summary>
        /// <param name="objectType">Type of object being serialized or deserialized.</param>
        /// <returns>A new instance implementing the <see cref="IMetaSerializer"/> interface.</returns>
        IMetaSerializer CreateSerializer( Type objectType );
    }
}