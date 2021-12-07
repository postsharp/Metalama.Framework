// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// An interface, common to <see cref="INamedType"/> and <see cref="IMethod"/>, that represents a generic declaration, i.e. a declaration
    /// with type parameters. This interface represents both generic definitions and generic instances. Generic declarations have a non-empty collection of <see cref="IGeneric.TypeParameters"/>.
    /// Generic definitions have an empty <see cref="TypeArguments"/> collection, while
    /// generic instances have the same number of items in <see cref="IGeneric.TypeParameters"/> and <see cref="TypeArguments"/>.
    /// </summary>
    /// <seealso cref="GenericExtensions"/>
    public interface IGeneric : IMemberOrNamedType
    {
        /// <summary>
        /// Gets the generic parameters of the current type or method.
        /// </summary>
        IGenericParameterList TypeParameters { get; }

        /// <summary>
        /// Gets the generic type arguments of the current type or method, which are the type values
        /// applied to the <see cref="TypeParameters"/> of the current type. This property returns
        /// an empty collection if the type or method an open generic definition or if the type or method is non-generic.
        /// </summary>
        IReadOnlyList<IType> TypeArguments { get; }

        /// <summary>
        /// Gets a value indicating whether this member or any of its containing types, if any, has any generic type argument that is not bound to a concrete value.
        /// </summary>
        bool IsOpenGeneric { get; }

        /// <summary>
        /// Gets a value indicating whether this member has generic parameters, regardless the fact that the containing type, if any, is generic.
        /// </summary>
        bool IsGeneric { get; }
    }
}