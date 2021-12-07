// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Types;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Allows to get instances of the <see cref="IType"/> interface or to test for type equality or inheritance.
    /// </summary>
    [CompileTimeOnly]
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
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="GenericExtensions.ConstructGenericInstance(Metalama.Framework.Code.INamedType,Metalama.Framework.Code.IType[])"/>.
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
        IArrayType ConstructArrayType( IType elementType, int rank );

        /// <summary>
        /// Creates a pointer type.
        /// </summary>
        /// <param name="pointedType"></param>
        /// <returns></returns>
        IPointerType ConstructPointerType( IType pointedType );

        T ConstructNullable<T>( T type )
            where T : IType;

        /// <summary>
        /// Gets a <see cref="INamedType"/> representing a given <see cref="SpecialType"/>.
        /// </summary>
        INamedType GetSpecialType( SpecialType specialType );

        /// <summary>
        /// Gets a run-time value that corresponds to the default value of a specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        dynamic? DefaultValue( IType type );

        /// <summary>
        /// Get a run-time value cast to a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        dynamic? Cast( IType type, object? value );

        /// <summary>
        /// Gets a declaration from a serialized identifier generated to <see cref="IRef{T}.ToSerializableId"/>.
        /// </summary>
        IDeclaration? GetDeclarationFromId( string declarationId );
    }
}