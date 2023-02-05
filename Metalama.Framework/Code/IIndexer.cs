// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Invokers;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code;

/// <summary>
/// Represents an indexer, i.e. a <c>this[*]</c> property.
/// </summary>
/// <seealso cref="IProperty"/>
public interface IIndexer : IPropertyOrIndexer, IHasParameters, IIndexerInvoker
{
    /// <summary>
    /// Gets a list of interface properties this property explicitly implements.
    /// </summary>
    IReadOnlyList<IIndexer> ExplicitInterfaceImplementations { get; }

    /// <summary>
    /// Gets an object that allows to invoke the current property.
    /// </summary>
    [Obsolete( "Use the RunTimeInvocationExtensions extension class.", true )]
    IInvokerFactory<IIndexerInvoker> Invokers { get; }

    /// <summary>
    /// Gets the base property that is overridden by the current property.
    /// </summary>
    IIndexer? OverriddenIndexer { get; }

    IIndexerInvoker With( InvokerOptions options );

    IIndexerInvoker With( dynamic target, InvokerOptions options = default );
}