// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public interface IGenericParameterBuilder : IDeclarationBuilder, IGenericParameter
    {
        /// <summary>
        /// Gets the type constraints of the generic parameter.
        /// </summary>
        new IList<IType> TypeConstraints { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter is covariant (i.e., <c>out</c>).
        /// </summary>
        new bool IsCovariant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter is contravariant (i.e., <c>in</c>).
        /// </summary>
        new bool IsContravariant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter has the <c>new()</c> constraint.
        /// </summary>
        new bool HasDefaultConstructorConstraint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter has the <c>class</c> constraint.
        /// </summary>
        new bool HasReferenceTypeConstraint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter has the <c>notnull</c> constraint.
        /// </summary>
        new bool HasNonNullableValueTypeConstraint { get; set; }
    }
}