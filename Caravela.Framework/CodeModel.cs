using System;
using System.Collections.Generic;
using Caravela.Reactive;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    public interface ICompilation
    {
        IReactiveCollection<INamedType> DeclaredTypes { get; }

        IReactiveCollection<INamedType> DeclaredAndReferencedTypes { get; }

        IReactiveGroupBy<string?, INamedType> DeclaredTypesByNamespace { get; }

        // TODO: assembly and module attributes? (do they need to be differentiated?)

        /// <summary>
        /// Get type based on its full name, as used in reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For nested types, this means using <c>+</c>, e.g. to get <see cref="System.Environment.SpecialFolder"/>, use <c>System.Environment+SpecialFolder</c>.
        /// </para>
        /// <para>
        /// For generic type definitions, this required using <c>`</c>, e.g. to get <see cref="List{T}"/>, use <c>System.Collections.Generic.List`1</c>.
        /// </para>
        /// <para>
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="INamedType.MakeGenericType"/>.
        /// </para>
        /// </remarks>
        INamedType? GetTypeByReflectionName(string reflectionName);

        INamedType? GetTypeByReflectionType( Type type );
    }

    public interface IType
    {
        bool Is(IType other);
    }

    // TODO: IArrayType etc.
    public interface INamedType : IType, ICodeElement
    {
        bool IsDefaultConstructible { get; }

        INamedType? BaseType { get; }

        IReactiveCollection<INamedType> ImplementedInterfaces { get; }

        string Name { get; }

        string? Namespace { get; }

        string FullName { get; }

        IReadOnlyList<IType> GenericArguments { get; }

        IReadOnlyList<IGenericParameter> GenericParameters { get; }


        // TODO: differentiate between class, struct and interface
        // TODO: separate NestedTypes and AllNestedTypes?
        IReactiveCollection<INamedType> NestedTypes { get; }

        // TODO: how to represent fields in general and compiler-generated backing fields in particular
        // don't show backing fields, ignore their attributes
        IReactiveCollection<IProperty> Properties { get; }

        IReactiveCollection<IProperty> AllProperties { get; }

        IReactiveCollection<IEvent> Events { get; }

        IReactiveCollection<IEvent> AllEvents { get; }

        // TODO: does this include accessor methods? yes, but classify them
        IReactiveCollection<IMethod> Methods { get; }

        IReactiveCollection<IMethod> AllMethods { get; }
    }

    public interface IAttribute
    {
        INamedType Type { get; }
        IReadOnlyList<object?> ConstructorArguments { get; }
        IReadOnlyDictionary<string, object?> NamedArguments { get; }
    }

    public interface ICodeElement
    {
        ICodeElement? ContainingElement { get; }
        IReactiveCollection<IAttribute> Attributes { get; }

        public CodeElementKind Kind { get; }
    }

    public enum CodeElementKind
    {
        None,
        Type,
        Method,
        Property,
        Field,
        Event,
        Parameter
    }

    public interface IMember : ICodeElement
    {
        string Name { get; }

        bool IsStatic { get; }

        bool IsVirtual { get; }

        INamedType? DeclaringType { get; }
    }

    public interface IProperty : IMember
    {
        // TODO: ref
        IType Type { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        IMethod? Getter { get; }
        // TODO: what happens if you try to set a get-only property in a constructor? it works, Setter returns pseudo elements for get-only
        // IsPseudoElement
        IMethod? Setter { get; }
    }

    public interface IEvent : IMember
    {
        INamedType DelegateType { get; }
        IMethod Adder { get; }
        IMethod Remover { get; }
        // TODO: how does this work? is it a "fake" method that invokes the underlying delegate for field-like events? yes
        IMethod? Raiser { get; }
    }

    public enum MethodKind
    {
        Ordinary,

        Constructor,
        StaticConstructor,
        Finalizer,

        PropertyGet,
        PropertySet,

        EventAdd,
        EventRemove,
        EventRaise,

        // DelegateInvoke
        // FunctionPointerSignature

        ExplicitInterfaceImplementation,

        ConversionOperator,
        UserDefinedOperator,

        LocalFunction,
    }

    public interface IMethod : IMember
    {
        IParameter ReturnParameter { get; }
        // for convenience
        IType ReturnType { get; }
        IReadOnlyList<IMethod> LocalFunctions { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        IReadOnlyList<IGenericParameter> GenericParameters { get; }
        new MethodKind Kind { get; }
    }

    public interface IParameter : ICodeElement
    {
        // TODO: should ref-ness be part of the type or the parameter? on parameter
        IType Type { get; }
        /// <remarks><see langword="null"/> for <see cref="IMethod.ReturnParameter"/></remarks>
        string? Name { get; }
        /// <remarks>-1 for <see cref="IMethod.ReturnParameter"/></remarks>
        int Index { get; }
        // TODO: default value?
    }

    public interface IGenericParameter : ICodeElement
    {
        string Name { get; }
        int Index { get; }
        IReadOnlyList<IType> BaseTypeConstraints { get; }
        bool IsCovariant { get; }
        bool IsContravariant { get; }
        bool HasDefaultConstructorConstraint { get; }
        bool HasReferenceTypeConstraint { get; }
        bool HasNotNullableValueTypeConstraint { get; }
        // TODO: nullable reference type constraints
    }
}
