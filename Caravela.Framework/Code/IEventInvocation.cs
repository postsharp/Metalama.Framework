﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows adding/removing delegates to/from events.
    /// </summary>
    [CompileTimeOnly]
    public interface IEventInvocation
    {
        /// <summary>
        /// Add a delegate to the event.
        /// </summary>
        [return: RunTimeOnly]
        dynamic AddDelegate( dynamic? instance, dynamic? value );

        /// <summary>
        /// Remove a delegate from the event.
        /// </summary>
        [return: RunTimeOnly]
        dynamic RemoveDelegate( dynamic? instance, dynamic? value );
    }
}