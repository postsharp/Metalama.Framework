using Caravela.Reactive;
using System.Collections.Generic;
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

        IReactiveCollection<INamedType> NestedTypes { get; }

        IReactiveCollection<IProperty> Properties { get; }

        IReactiveCollection<IEvent> Events { get; }

        IReactiveCollection<IMethod> Methods { get; }

        public INamedType MakeGenericType( params IType[] genericArguments );
    }
}