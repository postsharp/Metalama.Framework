// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// A base interface for all invokers, which are a mechanism to invoke the semantic of declarations and to specify which
    /// layer of overrides should be invoked.
    /// </summary>
    public interface IInvoker
    {
        /// <summary>
        /// Gets a value indicating which layer of the semantic must be invoked relatively to the current layer.
        /// </summary>
        InvokerOrder Order { get; }
    }
}