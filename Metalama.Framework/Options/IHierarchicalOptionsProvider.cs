// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

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
/// <remarks>
///  <para>This interface behaves differently when implemented by a plain custom attribute than when applied to an aspect.</para>
///  <para>When this interface is implemented by a plain custom attribute, the result of the <see cref="GetOptions"/> method is taken into account
/// when options are requested through the <see cref="DeclarationExtensions.Enhancements{T}"/>.<see cref="DeclarationEnhancements{T}.GetOptions{TOptions}"/> method.</para>
/// <para>However, when this interface is implemented by an aspect (i.e. any class implementing the <see cref="IAspect{T}"/> interface), the result of the
/// <see cref="GetOptions"/> of the aspect is <i>ignored</i> by <see cref="DeclarationExtensions.Enhancements{T}"/>.<see cref="DeclarationEnhancements{T}.GetOptions{TOptions}"/>
/// and only returned by <see cref="IAspectInstance"/>.<see cref="IAspectInstance.GetOptions{T}"/>.</para>
/// </remarks>
public interface IHierarchicalOptionsProvider<out T> : IHierarchicalOptionsProvider
    where T : class, IHierarchicalOptions
{
    /// <summary>
    /// Gets the options specified by the current attribute or aspect.
    /// </summary>
    T GetOptions();
}