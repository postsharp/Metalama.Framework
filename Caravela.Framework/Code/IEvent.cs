// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an event.
    /// </summary>
    public interface IEvent : IMember
    {
        /// <summary>
        /// Gets the type of the event, i.e. the type of the delegate.
        /// </summary>
        INamedType EventType { get; } 
        
        IMethod Signature { get; }

        /// <summary>
        /// Gets the method implementing the <c>add</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        IMethod Adder { get; }

        /// <summary>
        /// Gets the method implementing the <c>remove</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        IMethod Remover { get; }

        /// <summary>
        /// Gets an object that represents the <c>raise</c> semantic and allows to add aspects and advices
        /// as with a normal method.
        /// </summary>
        IMethod? Raiser { get; }

        /// <summary>
        /// Gets an object that allows to add or remove a handler to or from the current event. 
        /// </summary>
        IInvokerFactory<IEventInvoker> Invokers { get; }

        /// <summary>
        /// Gets a list of interface events this event explicitly implements.
        /// </summary>
        IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets an <see cref="EventInfo"/> that represents the current event at run time.
        /// </summary>
        /// <returns>An <see cref="EventInfo"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        EventInfo ToEventInfo();
    }
}