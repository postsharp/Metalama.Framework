// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code;

[CompileTime]
public static class MethodCollectionExtensions
{
    /// <summary>
    /// Gets the list of methods with signatures compatible with specified constraints.
    /// </summary>
    /// <param name="methods">A collection of methods.</param>
    /// <param name="name">Name of the method.</param>
    /// <param name="argumentTypes">Constraint on reflection types of arguments. <c>Null</c>items in the list signify any type.</param>
    /// <param name="isStatic">Constraint on staticity of the method.</param>
    /// <returns>Enumeration of methods matching specified constraints.</returns>
    public static IEnumerable<IMethod> OfCompatibleSignature(
        this IMethodCollection methods,
        string name,
        IReadOnlyList<Type?>? argumentTypes,
        bool? isStatic = false )
    {
        return methods.OfCompatibleSignature(
            (argumentTypes, (ICompilationInternal) methods.DeclaringType.Compilation),
            name,
            argumentTypes?.Count,
            GetParameter,
            isStatic );

        static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<Type?>? ArgumentTypes, ICompilationInternal Compilation) context, int index )
            => context.ArgumentTypes != null && context.ArgumentTypes[index] != null
                ? (context.Compilation.TypeFactory.GetTypeByReflectionType( context.ArgumentTypes[index]! ), null)
                : (null, null);
    }

    /// <summary>
    /// Gets the list of methods with signatures compatible with specified constraints.
    /// </summary>
    /// <param name="methods">A collection of methods.</param>
    /// <param name="name">Name of the method.</param>
    /// <param name="argumentTypes">Constraint on types of arguments. <c>Null</c>items in the list signify any type.</param>
    /// <param name="refKinds">Constraint on reference kinds of arguments. <c>Null</c>items in the list signify any reference kind.</param>
    /// <param name="isStatic">Constraint on staticity of the method.</param>
    /// <returns>Enumeration of methods matching specified constraints.</returns>
    public static IEnumerable<IMethod> OfCompatibleSignature(
        this IMethodCollection methods,
        string name,
        IReadOnlyList<IType?>? argumentTypes,
        IReadOnlyList<RefKind?>? refKinds = null,
        bool? isStatic = false )
    {
        return methods.OfCompatibleSignature(
            (argumentTypes, refKinds),
            name,
            argumentTypes?.Count,
            GetParameter,
            isStatic );

        static (IType? Type, RefKind? RefKind) GetParameter( (IReadOnlyList<IType?>? ArgumentTypes, IReadOnlyList<RefKind?>? RefKinds) context, int index )
            => (context.ArgumentTypes?[index], context.RefKinds?[index]);
    }

    /// <summary>
    /// Gets a method that exactly matches the specified signature.
    /// </summary>
    /// <param name="methods">A collection of methods.</param>
    /// <param name="name">Name of the method.</param>
    /// <param name="parameterTypes">List of parameter types.</param>
    /// <param name="refKinds">List of parameter reference kinds, or <c>null</c> if all parameters should be by-value.</param>
    /// <param name="isStatic">Staticity of the method.</param>
    /// <returns>A <see cref="IMethod"/> that matches the given signature.</returns>
    public static IMethod? OfExactSignature(
        this IMethodCollection methods,
        string name,
        IReadOnlyList<IType> parameterTypes,
        IReadOnlyList<RefKind>? refKinds = null,
        bool? isStatic = null )
    {
        return methods.OfExactSignature(
            (parameterTypes, refKinds),
            name,
            parameterTypes.Count,
            GetParameter,
            isStatic );

        static (IType Type, RefKind RefKind) GetParameter( (IReadOnlyList<IType> ParameterTypes, IReadOnlyList<RefKind>? RefKinds) context, int index )
            => (context.ParameterTypes[index], context.RefKinds?[index] ?? RefKind.None);
    }

    // TODO: Add this method:
    // IMethod? OfExactSignature(
    //     string name,
    //     int genericParameterCount,
    //     IReadOnlyList<Type> parameterTypes,
    //     bool? isStatic = null,
    //     bool declaredOnly = true );

    /// <summary>
    /// Gets a method that exactly matches the signature of the specified method.
    /// </summary>
    /// <param name="methods">A collection of methods.</param>
    /// <param name="signatureTemplate">Method signature of which to should be considered.</param>
    /// <param name="matchIsStatic">Value indicating whether the staticity of the method should be matched.</param>
    /// <returns>A <see cref="IMethod"/> that matches the given signature.</returns>
    public static IMethod? OfExactSignature( this IMethodCollection methods, IMethod signatureTemplate, bool matchIsStatic = true )
    {
        return methods.OfExactSignature(
            signatureTemplate,
            signatureTemplate.Name,
            signatureTemplate.Parameters.Count,
            GetParameter,
            matchIsStatic ? signatureTemplate.IsStatic : null );

        static (IType Type, RefKind RefKind) GetParameter( IMethod context, int index ) => (context.Parameters[index].Type, context.Parameters[index].RefKind);
    }

    /// <summary>
    /// Gets the list of methods of a given <see cref="MethodKind"/> (such as <see cref="MethodKind.ConversionOperator"/> or <see cref="MethodKind.Default"/>.
    /// </summary>
    public static IEnumerable<IMethod> OfKind( this IMethodCollection methods, MethodKind kind ) => methods.Where( m => m.MethodKind == kind );
}