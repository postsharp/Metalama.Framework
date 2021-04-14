// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

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
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface INamedType : IType, IMember
    {
        /// <summary>
        /// Gets a value indicating whether the type is marked as <c>partial</c> in source code. 
        /// </summary>
        bool IsPartial { get; }

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
        IReadOnlyList<INamedType> ImplementedInterfaces { get; }

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
        IReadOnlyList<IType> GenericArguments { get; }

        /// <summary>
        /// Gets the generic parameters of the type, or an empty collection if the
        /// type is not generic.
        /// </summary>
        IGenericParameterList GenericParameters { get; }

        /// <summary>
        /// Gets a value indicating whether this type or any of its containers does not have generic arguments set.
        /// </summary>
        bool IsOpenGeneric { get; }

        /// <summary>
        /// Gets the nested types of the current type.
        /// </summary>
        INamedTypeList NestedTypes { get; }

        /// <summary>
        /// Gets the list of properties defined by the current type, but not those inherited from the base type.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection.
        /// </summary>
        IPropertyList Properties { get; }

        /// <summary>
        /// Gets the list of fields defined by the current type, but not those inherited from the base  type.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection.
        /// </summary>
        IFieldList Fields { get; }

        /// <summary>
        /// Gets the union of the <see cref="Fields"/> and <see cref="Properties"/> collections.
        /// </summary>
        IFieldOrPropertyList FieldsAndProperties { get; }

        /// <summary>
        /// Gets the list of events defined by the current type, but not those inherited from the base
        /// type.
        /// </summary>
        IEventList Events { get; }

        /// <summary>
        /// Gets the list of methods defined by the current type, but not those inherited from the base
        /// type, and not constructors.
        /// </summary>
        IMethodList Methods { get; }

        /// <summary>
        /// Gets the list of constructors, including the implicit default constructor if any, but not the static constructor. 
        /// </summary>
        IConstructorList Constructors { get; }

        /// <summary>
        /// Gets the static constructor.
        /// </summary>
        IConstructor? StaticConstructor { get; }

        /// <summary>
        /// Makes a generic instance of the current generic type definition.
        /// </summary>
        /// <param name="genericArguments"></param>
        /// <returns></returns>
        INamedType WithGenericArguments( params IType[] genericArguments );

        /// <summary>
        /// Gets the assembly that declared this type.
        /// </summary>
        IAssembly DeclaringAssembly { get; }
    }
}