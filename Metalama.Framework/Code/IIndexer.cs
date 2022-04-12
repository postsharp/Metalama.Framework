// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Invokers;
using System.Collections.Generic;

namespace Metalama.Framework.Code;

/// <summary>
/// Represents an indexer.
/// </summary>
/// <seealso cref="IProperty"/>
public interface IIndexer : IPropertyOrIndexer, IHasParameters
{
    /// <summary>
    /// Gets a list of interface properties this property explicitly implements.
    /// </summary>
    IReadOnlyList<IIndexer> ExplicitInterfaceImplementations { get; }

    /// <summary>
    /// Gets an object that allows to invoke the current property.
    /// </summary>
    IInvokerFactory<IIndexerInvoker> Invokers { get; }

    /// <summary>
    /// Gets the base property that is overridden by the current property.
    /// </summary>
    IIndexer? OverriddenIndexer { get; }
}