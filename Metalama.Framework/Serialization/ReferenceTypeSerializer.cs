// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Serialization
{
    public abstract class ReferenceTypeSerializer : ISerializer
    {
        /// <inheritdoc />
        bool ISerializer.IsTwoPhase => true;

        /// <inheritdoc />
        void ISerializer.DeserializeFields( ref object obj, IArgumentsReader initializationArguments )
        {
            this.DeserializeFields( obj, initializationArguments );
        }

        /// <inheritdoc />
        void ISerializer.SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            this.SerializeObject( obj, constructorArguments, initializationArguments );
        }

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="type">Type of the instance to be created.</param>
        /// <param name="constructorArguments">Gives access to arguments required to create the instance.</param>
        /// <returns>An instance of type <paramref name="type"/> initialized using <paramref name="constructorArguments"/>.</returns>
        public abstract object CreateInstance( Type type, IArgumentsReader constructorArguments );

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="constructorArguments">Gives access to arguments that will be passed to the <see cref="CreateInstance"/> method during deserialization.</param>
        /// <param name="initializationArguments">Gives access to arguments that will be passed to the <see cref="DeserializeFields"/> method during deserialization.</param>
        public abstract void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments );

        /// <summary>
        /// Completes the second phase of deserialization by setting fields and other properties.
        /// </summary>
        /// <param name="obj">The object being deserialized.</param>
        /// <param name="initializationArguments">Gives access to field values.</param>
        public abstract void DeserializeFields( object obj, IArgumentsReader initializationArguments );

        /// <inheritdoc />
        public virtual object Convert( object value, Type targetType )
        {
            return value;
        }
    }
}