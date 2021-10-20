// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code.DeclarationBuilders
{
    public interface ITypeParameterBuilder : IDeclarationBuilder, ITypeParameter
    {
        new bool? IsConstraintNullable { get; set; }

        new string Name { get; set; }

        new TypeKindConstraint TypeKindConstraint { get; set; }

        new VarianceKind Variance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generic parameter has the <c>new()</c> constraint.
        /// </summary>
        new bool HasDefaultConstructorConstraint { get; set; }

        void AddTypeConstraint( IType type );

        void AddTypeConstraint( Type type );
    }
}