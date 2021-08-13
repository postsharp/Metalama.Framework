// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Invokers
{
    /// <summary>
    /// Allows adding/removing delegates to/from events.
    /// </summary>
    [CompileTimeOnly]
    public interface IEventInvoker : IInvoker
    {
        /// <summary>
        /// Add a delegate to the event.
        /// </summary>
        dynamic Add( dynamic? instance, dynamic? handler );

        /// <summary>
        /// Remove a delegate from the event.
        /// </summary>
        dynamic Remove( dynamic? instance, dynamic? handler );

        dynamic? Raise( dynamic? instance, params dynamic?[] args );
    }
}