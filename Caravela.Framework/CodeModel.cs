﻿using System.Collections.Generic;
using Caravela.Reactive;

// TODO: InternalImplement
namespace Caravela.Framework
{
    public interface ICompilation
    {
        IReactiveCollection<ITypeInfo> Types { get; }

        // TODO: assembly and module attributes? (do they need to be differentiated?)

        INamedType? GetTypeByMetadataName(string metadataName);
    }

    public interface IType
    {
        bool Is(IType other);
    }

    // TODO: IArrayType etc.
    public interface INamedType : IType, ICodeElement
    {
        string Name { get; }
        // TODO: how to deal with namespaces, especially considering nested types
        string FullName { get; }
        IReadOnlyList<IType> GenericArguments { get; }

        ITypeInfo GetTypeInfo(in ReactiveObserverToken observerToken);
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
    }

    // TODO: how to represent enums, delegates and records? like roslyn
    public interface ITypeInfo : INamedType, ICodeElement
    {
        // TODO: differentiate between class, struct and interface
        IReadOnlyList<ITypeInfo> NestedTypes { get; }
        // TODO: how to represent fields in general and compiler-generated backing fields in particular
        // don't show backing fields, ignore their attributes
        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IEvent> Events { get; }
        // TODO: does this include accessor methods? yes, but classify them
        IReadOnlyList<IMethod> Methods { get; }
        IReadOnlyList<IGenericParameter> GenericParameters { get; }
    }

    public interface IMember : ICodeElement
    {
        string Name { get; }
        bool IsStatic { get; }
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
        MethodKind Kind { get; }
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
