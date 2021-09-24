// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    /// <seealso cref="TypeExtensions"/>
    [CompileTimeOnly]
    public interface IType : ICompilationElement, IDisplayable
    {
        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets the <see cref="Code.SpecialType"/> enumeration value for the current type. Provides a fast to determine whether
        /// the current type is of a well-known type. 
        /// </summary>
        SpecialType SpecialType { get; }

        /// <summary>
        /// Gets a reflection <see cref="Type"/> that represents the current type at run time.
        /// </summary>
        /// <returns>A <see cref="Type"/> that can be used only in run-time code.</returns>
        Type ToType();
        
        /// <summary>
        /// Gets a value indicating whether the type is a reference type. If the type is a generic parameter
        /// without a <c>struct</c>, <c>class</c> or similar constraint, this property evaluates to <c>null</c>.
        /// </summary>
        bool? IsReferenceType { get; }
        
        /// <summary>
        /// Gets the nullability of the type, or <c>null</c> if the type is a reference type but its nullability has not been analyzed or specified.
        /// This property returns <c>false</c> for value types, including nullable value types, because a value type <c>T?</c> is represented
        /// as <c>Nullable&lt;T&gt;</c>.
        /// </summary>
        bool? IsNullable { get; }
    }
}