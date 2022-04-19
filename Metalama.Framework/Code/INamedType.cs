// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a class, struct, enum, or delegate.
    /// </summary>
    public interface INamedType : IType, IGeneric
    {
        /// <summary>
        /// Gets a value indicating whether the type is marked as <c>partial</c> in source code. 
        /// </summary>
        bool IsPartial { get; }

        /// <summary>
        /// Gets a value indicating whether the type is defined in a different project or assembly than the current <see cref="ICompilation"/>.
        /// </summary>
        bool IsExternal { get; }

        // TODO: there should probably be an interface to represent named tuples. It would be derived from INamedType
        // and be augmented by the names of tuple members.

        // TODO: the default constructor should be represented as a pseudo-method.
        bool HasDefaultConstructor { get; }

        /// <summary>
        /// Gets the type from which the current type derives.
        /// </summary>
        INamedType? BaseType { get; }

        /// <summary>
        /// Gets the list of all interfaces (recursive) that the current type implements.
        /// </summary>
        IImplementedInterfaceList AllImplementedInterfaces { get; }

        /// <summary>
        /// Gets the list of interfaces that the current type implements.
        /// </summary>
        IImplementedInterfaceList ImplementedInterfaces { get; }

        /// <summary>
        /// Gets the namespace of the current type. If the <see cref="IsExternal"/> property is <c>true</c>,
        /// this property throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        INamespace Namespace { get; }

        /// <summary>
        /// Gets the name of the type including its namespace.
        /// </summary>
        string FullName { get; }

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
        /// Gets the list of indexers defined in the current type.
        /// </summary>
        IIndexerList Indexers { get; }

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
        IConstructor StaticConstructor { get; }

        /// <summary>
        /// Gets a value indicating whether the type is <c>readonly</c>.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Determines whether the type if subclass of the given class or interface.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsSubclassOf( INamedType type );

        /// <summary>
        /// Finds the the implementation of the given interface member that is valid for this type.
        /// </summary>
        bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember );
    }
}