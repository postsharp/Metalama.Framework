using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IMethodInvocation
    {
        /// <summary>
        /// Allows invocation of the method.
        /// </summary>
        dynamic Invoke( dynamic? instance, params dynamic[] args );
    }

    /// <summary>
    /// Represents a method.
    /// </summary>
    public interface IMethod : IMember, IMethodInvocation
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

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IImmutableList<IParameter> Parameters { get; }

        /// <summary>
        /// Gets the generic parameters of the current method.
        /// </summary>
        IImmutableList<IGenericParameter> GenericParameters { get; }
        IImmutableList<IType> GenericArguments { get; }

        /// <summary>
        /// Indicates whether this method or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

        /// <summary>
        /// Used for generic invocations. It returns an IMethod, not an IMethodInvocation, because
        /// it may be useful to evaluate the bound return and parameter types.
        /// </summary>
        IMethod WithGenericArguments( params IType[] genericArguments );

        /// <summary>
        /// Determines if the method existed before the current aspect was advice
        /// (<see langword="false" /> if it was introduced by the current aspect).
        /// </summary>
        bool HasBase { get; }

        /// <summary>
        /// Allows invocation of the base method (<see langword="null" /> if the method was introduced by the current aspect).
        /// </summary>
        IMethodInvocation Base { get; }
    }
}