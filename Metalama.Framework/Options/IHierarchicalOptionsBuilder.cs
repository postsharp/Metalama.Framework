// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Options;

/// <summary>
/// Represents a mutable builder of an immutable <see cref="IHierarchicalOptions"/> object. This is the base non-generic interface
/// for the generic <see cref="IHierarchicalOptionsBuilder{T}"/>. Always implement the generic interface.
/// </summary>
[CompileTime]
public interface IHierarchicalOptionsBuilder
{
    /// <summary>
    /// Returns the immutable options.
    /// </summary>
    /// <returns></returns>
    IHierarchicalOptions Build();
}

/// <summary>
/// Represents a mutable builder of an immutable <see cref="IHierarchicalOptions"/> object.
/// Implement a generic instance of this interface for each type of declarations on which the options
/// are eligible.
/// </summary>
/// <typeparam name="T">The type of declarations on which the options are eligible.</typeparam>
public interface IHierarchicalOptionsBuilder<in T> : IHierarchicalOptionsBuilder
    where T : class, IDeclaration { }