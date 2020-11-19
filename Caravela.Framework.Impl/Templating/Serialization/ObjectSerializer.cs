using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// An object serializer can be registered with <see cref="ObjectSerializers"/> to serialize objects of a specific type into Roslyn creation expressions.
    /// </summary>
    public abstract class ObjectSerializer
    {
        /// <summary>
        /// Serializes an object of a type supported by this object serializer into a Roslyn expression that creates such an object.
        /// </summary>
        /// <param name="o">An object guaranteed to be of the type supported by this serializer.</param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax SerializeObject( object o );
    }
}