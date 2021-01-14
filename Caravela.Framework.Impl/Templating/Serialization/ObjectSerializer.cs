using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    /// <summary>
    /// An object serializer can be registered with <see cref="ObjectSerializers"/> to serialize objects of a specific type into Roslyn creation expressions.
    /// </summary>
    abstract class ObjectSerializer
    {
        /// <summary>
        /// Serializes an object of a type supported by this object serializer into a Roslyn expression that creates such an object.
        /// </summary>
        /// <param name="o">An object guaranteed to be of the type supported by this serializer.</param>
        /// <returns>An expression that creates such an object.</returns>
        public abstract ExpressionSyntax SerializeObject( object o );

        /// <summary>
        /// Throws a <see cref="CaravelaException"/> if we are in an infinite recursion cycle because of an attempt to serialize <paramref name="obj"/>.
        /// </summary>
        protected internal static void ThrowIfStackTooDeep(object obj)
        {
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
            }
            catch
            {
                throw new CaravelaException(GeneralDiagnosticDescriptors.CycleInSerialization, obj);
            }
        }
    }
}