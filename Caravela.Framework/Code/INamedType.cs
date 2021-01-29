using Caravela.Reactive;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a class, struct, enum, or delegate.
    /// </summary>
    public interface INamedType : IType, ICodeElement
    {
        // TODO: the default constructor should be represented as a pseudo-method.
        bool HasDefaultConstructor { get; }

        /// <summary>
        /// Gets the type from which the current type derives. If the base type is a generic type,
        /// the generic arguments are given by <see cref="GenericArguments"/>.
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
        /// Gets the generic type arguments of the current type with respect to the base type.
        /// </summary>
        IImmutableList<IType> GenericArguments { get; }

        /// <summary>
        /// Gets the generic parameters of the type.
        /// </summary>
        IImmutableList<IGenericParameter> GenericParameters { get; }

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
        public INamedType MakeGenericType( params IType[] genericArguments );
    }
}