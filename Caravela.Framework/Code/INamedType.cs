using Caravela.Reactive;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface INamedType : IType, ICodeElement
    {
        bool HasDefaultConstructor { get; }

        INamedType? BaseType { get; }

        IReactiveCollection<INamedType> ImplementedInterfaces { get; }

        string Name { get; }

        string? Namespace { get; }

        string FullName { get; }

        IImmutableList<IType> GenericArguments { get; }

        IImmutableList<IGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Indicates whether this type or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        IReactiveCollection<INamedType> NestedTypes { get; }

        IReactiveCollection<IProperty> Properties { get; }

        IReactiveCollection<IEvent> Events { get; }

        IReactiveCollection<IMethod> Methods { get; }

        public INamedType WithGenericArguments( params IType[] genericArguments );
    }
}