// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Comparers;

namespace Metalama.Framework.Code;

/// <summary>
/// Represents a reference to an <see cref="IDeclaration"/> or <see cref="IType"/>, which is valid across different compilation versions
/// (i.e. <see cref="ICompilation"/>) and, when serialized, across projects and processes. References can be resolved using <see cref="GetTarget"/>,
/// given an compilation, or using the <see cref="RefExtensions.GetTarget{T}"/> extension method for the compilation of the current context.
/// </summary>
/// <typeparam name="T">The type of the target object of the declaration or type.</typeparam>
/// <remarks>
/// <para>Use <see cref="RefEqualityComparer{T}"/> to compare instances of <see cref="IRef"/>.</para>
/// </remarks>
public interface IRef<out T> : IRef
    where T : class, ICompilationElement
{
    /// <summary>
    /// Gets the target of the reference for a given compilation, or throws an exception if the reference cannot be resolved. To get the reference for the
    /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
    /// </summary>
    new T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = null );

    /// <summary>
    /// Gets the target of the reference for a given compilation, or returns <c>null</c> if the reference cannot be resolved. To get the reference for the
    /// current execution context, use the <see cref="RefExtensions.GetTargetOrNull{T}"/> extension method.
    /// </summary>
    new T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = null );
}