// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a generic parameter of a method or type.
    /// </summary>
    public interface IGenericParameter : INamedDeclaration, IType
    {
        /// <summary>
        /// Gets the position of the generic parameter.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the type (interface) constraints of the generic parameter.
        /// </summary>
        IReadOnlyList<IType> TypeConstraints { get; }

        /// <summary>
        /// Gets the constraint on the kind of type, e.g. <see cref="Code.TypeKindConstraint.Class"/> or <see cref="Code.TypeKindConstraint.Struct"/>.
        /// </summary>
        TypeKindConstraint TypeKindConstraint { get; }

        /// <summary>
        /// Gets the kind variance: <see cref="VarianceKind.In"/>, <see cref="VarianceKind.Out"/> or <see cref="VarianceKind.None"/>.
        /// </summary>
        VarianceKind Variance { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Code.TypeKindConstraint.Class"/> constraint has the nullable annotation (?).
        /// This property returns <c>null</c> if the <see cref="TypeKindConstraint"/> has a different value than <see cref="Code.TypeKindConstraint.Class"/>
        /// or if the nullability of the generic parameter is not analyzed.
        /// </summary>
        bool? IsConstraintNullable { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter has the <c>new()</c> constraint.
        /// </summary>
        bool HasDefaultConstructorConstraint { get; }
    }
}