// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows adding/removing delegates to/from events.
    /// </summary>
    public interface IEventInvoker : IInvoker
    {
        /// <summary>
        /// Add a delegate to the event.
        /// </summary>
        dynamic Add( dynamic? handler );

        /// <summary>
        /// Remove a delegate from the event.
        /// </summary>
        dynamic Remove( dynamic? handler );

        dynamic? Raise( params dynamic?[] args );
    }
}