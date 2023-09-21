// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Options;

/// <summary>
/// A non-generic base interface for the generic <see cref="IHierarchicalOptions{T}"/>. You should always implement the generic interface.
/// </summary>
[RunTimeOrCompileTime]
public interface IHierarchicalOptionsProvider { }

/// <summary>
/// Interface, when implemented by any custom attribute (<see cref="System.Attribute"/>) or aspect (<see cref="IAspect"/>), means that this custom attribute
/// or aspect can provide an option layer.
/// </summary>
/// <typeparam name="T">The type of options provided by the current attribute or aspect.</typeparam>
public interface IHierarchicalOptionsProvider<out T> : IHierarchicalOptionsProvider
    where T : class, IHierarchicalOptions
{
    /// <summary>
    /// Gets the options specified by the current attribute or aspect.
    /// </summary>
    T GetOptions();
}