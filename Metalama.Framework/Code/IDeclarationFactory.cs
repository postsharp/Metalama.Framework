// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Types;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Allows to get instances of the <see cref="IType"/> interface or to test for type equality or inheritance.
    /// </summary>
    [CompileTime]
    internal interface IDeclarationFactory
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
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="GenericExtensions.WithTypeArguments(IMethod, IType[])"/>.
        /// </para>
        /// </remarks>
        INamedType GetTypeByReflectionName( string reflectionName );

        /// <summary>
        /// Gets an <see cref="IType"/> given a reflection <see cref="Type"/>.
        /// </summary>
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

        IType ConstructNullable( IType type, bool isNullable );

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
        IDeclaration GetDeclarationFromId( SerializableDeclarationId declarationId );

        ICompilationElement? Translate( ICompilationElement compilationElement, ReferenceResolutionOptions options = ReferenceResolutionOptions.Default );

        IType GetTypeFromId( SerializableTypeId serializableTypeId, IReadOnlyDictionary<string, IType>? genericArguments );
    }
}