using System;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    [CompileTime]
    public interface IType : IDisplayable
    {
        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as an <see cref="IType"/>.
        /// </summary>
        /// <param name="other">Another type.</param>
        /// <returns></returns>
        bool Is( IType other );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="other">Another type.</param>
        /// <returns></returns>
        bool Is( Type other );

        // TODO: Define Is(string)
        // TODO: Define Equals

        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <param name="rank">Rank of the array/.</param>
        /// <returns>An array type <c>T[]</c> where <c>T</c> is the current type.</returns>
        IArrayType MakeArrayType( int rank = 1 );

        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <returns>An unsafe pointer type <c>*T</c> where <c>T</c> is the current type.</returns>
        IPointerType MakePointerType();
    }
}