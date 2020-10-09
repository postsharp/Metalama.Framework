using System.Collections.Generic;

namespace PostSharp.Framework
{
    public interface ICompilation
    {
        IReadOnlyList<ITypeInfo> Types { get; }

        // TODO: assembly and module attributes? (do they need to be differentiated?)
    }

    // TODO: should this be abstract class, so that users are not tempted to implement it?
    public interface ITypeResolutionToken { }

    public interface IType
    {
    }

    public interface INamedType : IType
    {
        string Name { get; }
        // TODO: how to deal with namespaces, especially considering nested types
        string FullName { get; }
        IReadOnlyList<IType> GenericArguments { get; }

        ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken);
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
        IReadOnlyList<IAttribute> Attributes { get; }
    }

    // TODO: how to represent enums, delegates and records
    public interface ITypeInfo : INamedType, ICodeElement
    {
        // TODO: differentiate between class, struct and interface
        IReadOnlyList<ITypeInfo> NestedTypes { get; }
        // TODO: how to represent fields in general and compiler-generated backing fields in particular
        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IEvent> Events { get; }
        // TODO: does this include accessor methods?
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
        IType Type { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        IMethod? Getter { get; }
        // TODO: what happens if you try to set a get-only property in a constructor?
        IMethod? Setter { get; }
    }

    public interface IEvent : IMember
    {
        INamedType DelegateType { get; }
        IMethod Adder { get; }
        IMethod Remover { get; }
        // TODO: how does this work? is it a "fake" method that invokes the underlying delegate for field-like events?
        IMethod? Invoker { get; }
    }

    public interface IMethod : IMember
    {
        IParameter ReturnParameter { get; }
        // for convenience
        IType ReturnType { get; }
        IReadOnlyList<IMethod> LocalFunctions { get; }
        IReadOnlyList<IParameter> Parameters { get; }
        IReadOnlyList<IGenericParameter> GenericParameters { get; }
    }

    public interface IParameter : ICodeElement
    {
        // TODO: should ref-ness be part of the type?
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
        // TODO: do special constraints this way, or do something weird, like reflection?
        bool IsCovariant { get; }
        bool IsContravariant { get; }
        bool HasDefaultConstructorConstraint { get; }
        bool HasReferenceTypeConstraint { get; }
        bool HasNotNullableValueTypeConstraint { get; }
        // TODO: nullable reference type constraints
    }
}
