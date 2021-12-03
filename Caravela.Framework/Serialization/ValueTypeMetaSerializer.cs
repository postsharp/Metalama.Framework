// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Serialization
{
    public abstract class ValueTypeMetaSerializer<T> : IMetaSerializer
        where T : struct
    {
        bool IMetaSerializer.IsTwoPhase => false;

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="constructorArguments">Gives access to arguments that will be passed to the <see cref="DeserializeObject"/> method during deserialization.</param>
        public abstract void SerializeObject( T obj, IArgumentsWriter constructorArguments );

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="constructorArguments">Gives access to arguments required to create the instance.</param>
        /// <returns>An instance of type <typeparamref name="T"/> initialized using <paramref name="constructorArguments"/>.</returns>
        public abstract T DeserializeObject( IArgumentsReader constructorArguments );

        /// <inheritdoc />
        void IMetaSerializer.SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var typedValue = (T) obj;
            this.SerializeObject( typedValue, constructorArguments );
        }

        /// <inheritdoc />
        public virtual object Convert( object value, Type targetType )
        {
            return value;
        }

        /// <inheritdoc />
        object IMetaSerializer.CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            return this.DeserializeObject( constructorArguments );
        }

        /// <inheritdoc />
        void IMetaSerializer.DeserializeFields( ref object o, IArgumentsReader initializationArguments )
        {
        }
    }
}