// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Code
{
    public enum RefComparison
    {
        Default,
        Structural
    }

    public enum RefOptions
    {
        Default,
        CompilationNeutral
    }

    /// <summary>
    /// Represents a reference to an <see cref="IDeclaration"/> or <see cref="IType"/>, which is valid across different compilation versions
    /// (i.e. <see cref="ICompilation"/>) and, when serialized, across projects and processes. References can be resolved using <see cref="GetTarget"/>,
    /// given an compilation, or using the <see cref="RefExtensions.GetTarget{T}"/> extension method for the compilation of the current context.
    /// All objects implementing this interface also implement the stronly-typed <see cref="IRef{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>Use <see cref="RefEqualityComparer{T}"/> to compare instances of <see cref="IRef"/>.</para>
    /// </remarks> 
    [CompileTime]
    [InternalImplement]
    public interface IRef : IEquatable<IRef>
    {
        /// <summary>
        /// Returns a string that uniquely identifies the declaration represented by the current reference. This identifier can then be resolved using <see cref="IDeclarationFactory.GetDeclarationFromId"/>, even in
        /// a different process or with a different version of Metalama than the one that created the id.
        /// </summary>
        /// <returns>A string, or <c>null</c> if the current reference cannot be serialized to a public id.</returns>
        SerializableDeclarationId ToSerializableId();

        IRef<TOut> As<TOut>()
            where TOut : class, ICompilationElement;

        /// <summary>
        /// Gets the target of the reference for a given compilation, or throws an exception if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
        /// </summary>
        ICompilationElement GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default );

        /// <summary>
        /// Gets the target of the reference for a given compilation, or returns <c>null</c> if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTargetOrNull{T}"/> extension method.
        /// </summary>
        ICompilationElement? GetTargetOrNull(
            ICompilation compilation,
            ReferenceResolutionOptions options = default,
            IGenericContext? genericContext = default );

        bool IsCompilationNeutral { get; }

        IRef ToCompilationNeutral();

        bool Equals( IRef? other, RefComparison comparison = RefComparison.Default );

        int GetHashCode( RefComparison comparison );
    }

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
        new T GetTarget( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default );

        /// <summary>
        /// Gets the target of the reference for a given compilation, or returns <c>null</c> if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTargetOrNull{T}"/> extension method.
        /// </summary>
        new T? GetTargetOrNull( ICompilation compilation, ReferenceResolutionOptions options = default, IGenericContext? genericContext = default );

        new IRef<T> ToCompilationNeutral();
    }
}