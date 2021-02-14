using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method.
    /// </summary>
    public interface IMethod : IMember, IMethodInvocation
    {
        /// <summary>
        /// Gets an object representing the method return type and custom attributes, or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </summary>
        IParameter? ReturnParameter { get; }

        /// <summary>
        /// Gets the method return type.
        /// </summary>
        IType? ReturnType { get; }

        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IReadOnlyList<IMethod> LocalFunctions { get; }

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IReadOnlyList<IParameter> Parameters { get; }

        /// <summary>
        /// Gets the generic parameters of the current method.
        /// </summary>
        IReadOnlyList<IGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Gets the generic arguments of the current method.
        /// </summary>
        IReadOnlyList<IType> GenericArguments { get; }

        /// <summary>
        /// Gets a value indicating whether this method or any of its containers does not have generic arguments set.
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
        /// Gets a value indicating whether the method existed before the current aspect was advice
        /// (<see langword="false" /> if it was introduced by the current aspect).
        /// </summary>
        bool HasBase { get; }

        /// <summary>
        /// Gets an object that allows invocation of the base method (<see langword="null" /> if the method was introduced by the current aspect).
        /// </summary>
        IMethodInvocation Base { get; }
    }
}