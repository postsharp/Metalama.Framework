// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    /// <summary>
    /// As <see cref="ObjectSerializer"/>, except strongly-typed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class TypedObjectSerializer<T> : ObjectSerializer
    {
        /// <inheritdoc />
        public sealed override ExpressionSyntax SerializeObject( object o )
        {
            return this.Serialize( (T) o );
        }

        /// <summary>
        /// Serializes an object of a type supported by this object serializer into a Roslyn expression that creates such an object.
        /// </summary>
        /// <param name="o">An object to serialize. Not null.</param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax Serialize( T o );
    }
}