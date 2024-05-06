// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Code;

/// <summary>
/// Exposes methods that return instances of the <see cref="IType"/> interface.
/// </summary>
[CompileTime]
public static class TypeFactory
{
    internal static IDeclarationFactory Implementation
    {
        get
        {
            var syntaxBuilder =
                MetalamaExecutionContext.CurrentInternal.SyntaxBuilder
                ?? throw new InvalidOperationException(
                    "TypeFactory is not available in this context. In BuildEligibility, TypeFactory can only be used inside eligibility delegates." );

            return ((ICompilationInternal) syntaxBuilder.Compilation).Factory;
        }
    }

    /// <summary>
    /// Gets an <see cref="IType"/> given a reflection <see cref="Type"/>.
    /// </summary>
    public static IType GetType( Type type ) => Implementation.GetTypeByReflectionType( type );

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
    public static INamedType GetType( string typeName ) => Implementation.GetTypeByReflectionName( typeName );

    /// <summary>
    /// Gets a <see cref="INamedType"/> representing a given <see cref="SpecialType"/>.
    /// </summary>
    public static INamedType GetType( SpecialType type ) => Implementation.GetSpecialType( type );

    /// <summary>
    /// Creates an array type from the current type.
    /// </summary>
    /// <param name="elementType">Type of array elements.</param>
    /// <param name="rank">Rank of the array/.</param>
    /// <returns>An array type <c>T[]</c> where <c>T</c> is the current type.</returns>
    public static IArrayType MakeArrayType( this IType elementType, int rank = 1 )
        => ((ICompilationInternal) elementType.Compilation).Factory.ConstructArrayType( elementType, rank );

    /// <summary>
    /// Creates an array type from the current type.
    /// </summary>
    /// <returns>An unsafe pointer type <c>T*</c> where <c>T</c> is the current type.</returns>
    public static IPointerType MakePointerType( this IType pointedType )
        => ((ICompilationInternal) pointedType.Compilation).Factory.ConstructPointerType( pointedType );

    /// <summary>
    /// Creates a nullable type from the current <see cref="IType"/>. If the current type is already nullable, returns the current type.
    /// If the type is a value type, returns a <see cref="Nullable{T}"/> of this type.
    /// </summary>
    public static IType ToNullableType( this IType type )
        => ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, true );
    
    /// <summary>
    /// Creates a nullable type from the current <see cref="INamedType"/>. If the current type is already nullable, returns the current type.
    /// If the type is a value type, returns a <see cref="Nullable{T}"/> of this type.
    /// </summary>
    public static INamedType ToNullableType( this INamedType type )
        => (INamedType) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, true );
    
    /// <summary>
    /// Creates a nullable type from the current <see cref="IArrayType"/>. If the current type is already nullable, returns the current type.
    /// </summary>
    public static IArrayType ToNullableType( this IArrayType type )
        => (IArrayType) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, true );

    /// <summary>
    /// Creates a nullable type from the current <see cref="IDynamicType"/>. If the current type is already nullable, returns the current type.
    /// </summary>
    public static IDynamicType ToNullableType( this IDynamicType type )
        => (IDynamicType) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, true );

    /// <summary>
    /// Returns the non-nullable type from the current <see cref="IType"/>. If the current type is a non-nullable reference type, returns the current type.
    /// If the current type is a <see cref="Nullable{T}"/>, i.e. a nullable value type, returns the underlying type.
    /// </summary>
    /// <remarks>
    /// Note that for non-value type type parameters, this method strips the nullable annotation, if any,
    /// which means it returns a type whose <see cref="IType.IsNullable"/> property returns <see langword="null" />.
    /// This is because C# has no way to express non-nullability for type parameters.
    /// </remarks>
    public static IType ToNonNullableType( this IType type )
        => ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, false );

    /// <summary>
    /// Returns the non-nullable type from the current <see cref="ITypeParameter"/>. If the current type is a non-nullable reference type, returns the current type.
    /// </summary>
    /// <remarks>
    /// Note that for non-value type type parameters, this method strips the nullable annotation, if any,
    /// which means it returns a type whose <see cref="IType.IsNullable"/> property returns <see langword="null" />.
    /// This is because C# has no way to express non-nullability for type parameters.
    /// </remarks>
    public static ITypeParameter ToNonNullableType( this ITypeParameter type )
        => (ITypeParameter) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, false );
    
    /// <summary>
    /// Returns the non-nullable type from the current <see cref="IArrayType"/>. If the current type is non-nullable, returns the current type.
    /// </summary>
    public static IArrayType ToNonNullableType( this IArrayType type )
        => (IArrayType) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, false );
    
    /// <summary>
    /// Returns the non-nullable type from the current <see cref="IDynamicType"/>. If the current type is non-nullable, returns the current type.
    /// </summary>
    public static IDynamicType ToNonNullableType( this IDynamicType type )
        => (IDynamicType) ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, false );

    /// <summary>
    /// Returns the non-nullable type from the current <see cref="INamedType"/>. If the current type is a non-nullable reference type, returns the current type.
    /// If current type represents a <see cref="Nullable{T}"/> where <c>T</c> is a value-type type parameter, this method returns <see cref="ITypeParameter"/>.
    /// Otherwise, it returns an <see cref="INamedType"/>.
    /// </summary>
    public static IType ToNonNullableType( this INamedType type )
        => ((ICompilationInternal) type.Compilation).Factory.ConstructNullable( type, false );
}