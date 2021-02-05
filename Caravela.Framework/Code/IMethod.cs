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

    public interface IMethod : IMember, IMethodInvocation
    {
        /// <remarks>
        /// Returns <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </remarks>
        IParameter? ReturnParameter { get; }
        // for convenience
        IType ReturnType { get; }
        IImmutableList<IMethod> LocalFunctions { get; }
        IImmutableList<IParameter> Parameters { get; }
        IImmutableList<IGenericParameter> GenericParameters { get; }
        IImmutableList<IType> GenericArguments { get; }

        /// <summary>
        /// Indicates whether this method or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        new MethodKind Kind { get; }

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