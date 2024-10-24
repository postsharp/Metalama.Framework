// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Comparers;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IType"/> interface implements <see cref="IEquatable{T}"/>. The implementation uses the <see cref="ICompilationComparers.Default"/> comparer.
    /// To use a different comparer, choose a different comparer from <see cref="IDeclaration"/>.<see cref="ICompilationElement.Compilation"/>.<see cref="ICompilation.Comparers"/>.
    /// You can also use <see cref="Equals(IType,TypeComparison)"/> and specify a <see cref="TypeComparison"/>.
    /// </remarks>
    /// <seealso cref="TypeExtensions"/>
    [CompileTime]
    public interface IType : ICompilationElement, IDisplayable, IEquatable<IType>
    {
        IRef<IType> ToRef();

        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets the <see cref="Code.SpecialType"/> enumeration value for the current type. Provides a fast way to determine whether
        /// the current type is of a well-known type. 
        /// </summary>
        SpecialType SpecialType { get; }

        /// <summary>
        /// Gets a reflection <see cref="Type"/> that represents the current type at run time.
        /// </summary>
        /// <returns>A <see cref="Type"/> that can be used only in run-time code.</returns>
        [CompileTimeReturningRunTime]
        Type ToType();

        /// <summary>
        /// Gets a value indicating whether the type is a reference type. If the type is a generic parameter
        /// without a <c>struct</c>, <c>class</c> or similar constraint, this property evaluates to <c>null</c>.
        /// </summary>
        bool? IsReferenceType { get; }

        /// <summary>
        /// Gets the nullability of the type, or <c>null</c> if the type is a reference type but its nullability has not been analyzed or specified.
        /// This property returns <c>false</c> for normal value types and <c>true</c> for the <see cref="Nullable{T}"/> type. Note that in
        /// case of nullable value types, the current type represents the <see cref="Nullable{T}"/> type itself, and the inner value type
        /// is exposed as <see cref="INamedType.UnderlyingType"/>.
        /// </summary>
        bool? IsNullable { get; }

        /// <summary>
        /// Determines whether the current type is equal to a well-known special type.
        /// </summary>
        bool Equals( SpecialType specialType );

        bool Equals( IType? otherType, TypeComparison typeComparison );
    }
}