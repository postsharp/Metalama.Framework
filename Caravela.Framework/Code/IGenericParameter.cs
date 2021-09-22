// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public enum TypeKindConstraint
    {
        None,
        Class,
        NullableClass,
        Struct,
        Unmanaged,
        NotNull,
        Default
    }

    public enum VarianceKind
    {
        None,
        In,
        Out
    }
    
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
        /// Gets the type constraints of the generic parameter.
        /// </summary>
        IReadOnlyList<IType> TypeConstraints { get; }

        TypeKindConstraint TypeKindConstraint { get; }
        
        VarianceKind Variance { get; }

        /// <summary>
        /// Gets a value indicating whether the generic parameter has the <c>new()</c> constraint.
        /// </summary>
        bool HasDefaultConstructorConstraint { get; }
    }
}