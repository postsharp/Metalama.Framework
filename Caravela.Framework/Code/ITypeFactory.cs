// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Types;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to get instances of the <see cref="IType"/> interface or to test for type equality or inheritance.
    /// </summary>
    public interface ITypeFactory
    {
        /// <summary>
        /// Get type based on its full name, as used in reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For nested types, this means using <c>+</c>, e.g. to get <see cref="System.Environment.SpecialFolder"/>, use <c>System.Environment+SpecialFolder</c>.
        /// </para>
        /// <para>
        /// For generic type definitions, this requires using <c>`</c>, e.g. to get <c>List&lt;T&gt;</c>, use <c>System.Collections.Generic.List`1</c>.
        /// </para>
        /// <para>
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="INamedType.WithGenericArguments(IType[])"/>.
        /// </para>
        /// </remarks>
        INamedType GetTypeByReflectionName( string reflectionName );

        /// <summary>
        /// Gets an <see cref="IType"/> given a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IType GetTypeByReflectionType( Type type );

        /// <summary>
        /// Creates an array type.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        IArrayType MakeArrayType( IType elementType, int rank );

        /// <summary>
        /// Creates a pointer type.
        /// </summary>
        /// <param name="pointedType"></param>
        /// <returns></returns>
        IPointerType MakePointerType( IType pointedType );

        T MakeNullable<T>( T type )
            where T : IType;

        IType GetSpecialType( SpecialType specialType );

        /// <summary>
        /// Gets a run-time value that corresponds to the default value of a specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        dynamic? DefaultValue( IType type );

        dynamic? Cast( IType type, object? value );
    }
}