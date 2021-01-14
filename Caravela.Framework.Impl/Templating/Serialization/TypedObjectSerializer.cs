using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// As <see cref="ObjectSerializer"/>, except strongly-typed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obfuscation( Exclude = true )]
    abstract class TypedObjectSerializer<T> : ObjectSerializer
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