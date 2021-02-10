using System.Collections.Immutable;
using Caravela.Reactive;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a class, struct, enum, or delegate.
    /// </summary>
    /// <remarks>
    /// <para>This code model represents both generic type definitions and generic type instances
    /// with the <see cref="INamedType"/>. Generic types have a non-empty collection of <see cref="GenericParameters"/>.
    /// Generic type definitions have an empty <see cref="GenericArguments"/> collection, while
    /// generic type instances have the same number of items in <see cref="GenericParameters"/> and <see cref="GenericArguments"/>.
    /// </para>
    /// </remarks>
    public interface INamedType : IType, ICodeElement
    {
        // TODO: there should probably be an interface to represent named tuples. It would be derived from INamedType
        // and be augmented by the names of tuple members.

        // TODO: the default constructor should be represented as a pseudo-method.
        bool HasDefaultConstructor { get; }

        /// <summary>
        /// Gets the type from which the current type derives.
        /// </summary>
        INamedType? BaseType { get; }

        /// <summary>
        /// Gets the list of interfaces that the current type implements.
        /// </summary>
        IReactiveCollection<INamedType> ImplementedInterfaces { get; }

        /// <summary>
        /// Gets the name of the type, but not its namespace.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the namespace of the current type.
        /// </summary>
        string? Namespace { get; }

        /// <summary>
        /// Gets the name of the type including its namespace.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the generic type arguments of the current type, which are the type values
        /// applied to the <see cref="GenericParameters"/> of the current type. Returns
        /// an empty collection if the type an open generic type definition or if the type is non-generic.
        /// </summary>
        IImmutableList<IType> GenericArguments { get; }

        /// <summary>
        /// Gets the generic parameters of the type, or an empty collection if the
        /// type is not generic.
        /// </summary>
        IImmutableList<IGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Gets a value indicating whether this type or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        /// <summary>
        /// Gets the nested types of the current type.
        /// </summary>
        IReactiveCollection<INamedType> NestedTypes { get; }

        /// <summary>
        /// Gets the list of properties and fields defined by the current type, but not those inherited from the base
        /// type.
        /// </summary>
        IReactiveCollection<IProperty> Properties { get; }

        /// <summary>
        /// Gets the list of events defined by the current type, but not those inherited from the base
        /// type.
        /// </summary>
        IReactiveCollection<IEvent> Events { get; }

        /// <summary>
        /// Gets the list of methods defined by the current type, but not those inherited from the base
        /// type.
        /// </summary>
        IReactiveCollection<IMethod> Methods { get; }

        /// <summary>
        /// Makes a generic instance of the current generic type definition.
        /// </summary>
        /// <param name="genericArguments"></param>
        /// <returns></returns>
        public INamedType WithGenericArguments( params IType[] genericArguments );
    }
}