using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method.
    /// </summary>
    public interface IMethod : IMember
    {
        /// <remarks>
        /// Gets an object representing the method return type and custom attributes, or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </remarks>
        IParameter? ReturnParameter { get; }
        
        /// <summary>
        /// Gets the method return type.
        /// </summary>
        IType ReturnType { get; }
        
        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IImmutableList<IMethod> LocalFunctions { get; }
        
        
        // TODO: Consider exposing several return parameters in case we have named tuples. We don't have a good way to represent tuples.
        
        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IImmutableList<IParameter> Parameters { get; }
        
        /// <summary>
        /// Gets the generic parameters of the current method.
        /// </summary>
        IImmutableList<IGenericParameter> GenericParameters { get; }
        
        /// <summary>
        /// Gets the kind of method (such as <see cref="MethodKind.Default"/> or <see cref="MethodKind.PropertyGet"/>.
        /// </summary>
        new MethodKind Kind { get; }

        //dynamic Invoke(params object[] args);
    }
}