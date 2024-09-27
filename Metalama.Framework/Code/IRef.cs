// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Code
{
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

        /// <summary>
        /// Changes the reference type. This method can be used in two scenarios: instead of a C# cast with durable references (see <see cref="IsDurable"/>),
        /// or between <see cref="IField"/> and <see cref="IProperty"/> when a field is overridden into a property (see <see cref="IField.OverridingProperty"/>
        /// and <see cref="IProperty.OriginalField"/>).
        /// </summary>
        IRef<TOut> As<TOut>()
            where TOut : class, ICompilationElement;

        /// <summary>
        /// Gets a value indicating whether the reference can be kept in memory without keeping a reference to the state of the project.
        /// Most references are bound to a specific state of the project. They are faster to resolve but prevent that specific project state to be garbage-collected.
        /// Durable references are slower to resolve but not cause a memory leak if they stay in memory for a long time.
        /// </summary>
        bool IsDurable { get; }

        /// <summary>
        /// Gets the target of the reference for a given compilation, or throws an exception if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTarget{T}"/> extension method.
        /// </summary>
        ICompilationElement GetTarget( ICompilation compilation, IGenericContext? genericContext = null );

        /// <summary>
        /// Gets the target of the reference for a given compilation, or returns <c>null</c> if the reference cannot be resolved. To get the reference for the
        /// current execution context, use the <see cref="RefExtensions.GetTargetOrNull{T}"/> extension method.
        /// </summary>
        ICompilationElement? GetTargetOrNull(
            ICompilation compilation,
            IGenericContext? genericContext = null );

        bool Equals( IRef? other, RefComparison comparison = RefComparison.Default );

        int GetHashCode( RefComparison comparison );

        DeclarationKind DeclarationKind { get; }

        string Name { get; }
    }
}