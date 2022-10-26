// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
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
        IImplementedInterfaceCollection AllImplementedInterfaces { get; }

        /// <summary>
        /// Gets the list of interfaces that the current type implements.
        /// </summary>
        IImplementedInterfaceCollection ImplementedInterfaces { get; }

        /// <summary>
        /// Gets the namespace of the current type.
        /// </summary>
        INamespace Namespace { get; }

        /// <summary>
        /// Gets the name of the type including its namespace.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the nested types of the current type.
        /// </summary>
        INamedTypeCollection NestedTypes { get; }

        /// <summary>
        /// Gets the list of properties defined in the current type, but not those inherited from the base types.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection.
        /// </summary>
        IPropertyCollection Properties { get; }

        /// <summary>
        /// Gets the list of properties defined in the current type or inherited from the base types.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection. 
        /// </summary>
        IPropertyCollection AllProperties { get; }

        /// <summary>
        /// Gets the list of indexers defined in the current type.
        /// </summary>
        IIndexerCollection Indexers { get; }

        /// <summary>
        /// Gets the list of indexers defined in the current type or inherited from the base types.
        /// </summary>
        IIndexerCollection AllIndexers { get; }

        /// <summary>
        /// Gets the list of fields defined in the current type, but not those inherited from the base type.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection.
        /// </summary>
        IFieldCollection Fields { get; }

        /// <summary>
        /// Gets the list of fields defined in the current type or inherited from the base types.
        /// Note that fields can be promoted to properties by aspects, so a source code field can be 
        /// represented in the <see cref="Properties" /> collection instead of the <see cref="Fields"/>
        /// collection. 
        /// </summary>
        IFieldCollection AllFields { get; }

        /// <summary>
        /// Gets the union of the <see cref="Fields"/> and <see cref="Properties"/> collections.
        /// </summary>
        IFieldOrPropertyCollection FieldsAndProperties { get; }

        /// <summary>
        /// Gets the union of the <see cref="AllFields"/> and <see cref="AllProperties"/> collections.
        /// </summary>
        IFieldOrPropertyCollection AllFieldsAndProperties { get; }

        /// <summary>
        /// Gets the list of events defined in the current type, but not those inherited from the base
        /// types.
        /// </summary>
        IEventCollection Events { get; }

        /// <summary>
        /// Gets the list of events defined in the current type or inherited from the base types.
        /// </summary>
        IEventCollection AllEvents { get; }

        /// <summary>
        /// Gets the list of methods defined in the current type, but not those inherited from the base
        /// type, and not constructors, finalizer or operators.
        /// </summary>
        IMethodCollection Methods { get; }

        /// <summary>
        /// Gets the list of methods defined in the current type or inherited from the base type.
        /// </summary>
        IMethodCollection AllMethods { get; }

        /// <summary>
        /// Gets the list of constructors, including the implicit default constructor if any, but not the static constructor. 
        /// </summary>
        IConstructorCollection Constructors { get; }

        /// <summary>
        /// Gets the static constructor.
        /// </summary>
        IConstructor? StaticConstructor { get; }

        /// <summary>
        /// Gets the finalizer of the type. For value types returns <c>null</c>.
        /// </summary>
        IMethod? Finalizer { get; }

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

        /// <summary>
        /// Gets the type definition with unassigned type parameters. When the current <see cref="INamedType"/> is not a generic type instance ,
        /// returns the current <see cref="IMethod"/>.
        /// </summary>
        INamedType TypeDefinition { get; }

        /// <summary>
        /// Gets the underlying type of an enum, the non-nullable type of a nullable type, or the current type.
        /// </summary>
        INamedType UnderlyingType { get; }
    }
}