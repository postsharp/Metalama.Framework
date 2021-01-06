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


        // TODO: differentiate between class, struct and interface
        // TODO: separate NestedTypes and AllNestedTypes?
        IReactiveCollection<INamedType> NestedTypes { get; }

        IReactiveCollection<IProperty> Properties { get; }

        IReactiveCollection<IProperty> AllProperties { get; }

        IReactiveCollection<IEvent> Events { get; }

        IReactiveCollection<IEvent> AllEvents { get; }

        // TODO: does this include accessor methods? yes, but classify them
        IReactiveCollection<IMethod> Methods { get; }

        IReactiveCollection<IMethod> AllMethods { get; }
    }
}