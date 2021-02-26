// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a generic parameter of a method or type.
    /// </summary>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IGenericParameter : ICodeElement, IType
    {
        /// <summary>
        /// Gets the generic parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the position of the generic parameter.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the type constraints of the generic parameter.
        /// </summary>
        IReadOnlyList<IType> TypeConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter is covariant (i.e., <c>out</c>).
        /// </summary>
        bool IsCovariant { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter is contravariant (i.e., <c>in</c>).
        /// </summary>
        bool IsContravariant { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter has the <c>new()</c> constraint.
        /// </summary>
        bool HasDefaultConstructorConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter has the <c>class</c> constraint.
        /// </summary>
        bool HasReferenceTypeConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter has the <c>notnull</c> constraint.
        /// </summary>
        bool HasNonNullableValueTypeConstraint { get; }

        // TODO: nullable reference type constraints
        // TODO: Unmanaged
    }
}