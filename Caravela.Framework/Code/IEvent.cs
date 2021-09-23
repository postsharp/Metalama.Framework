// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an event.
    /// </summary>
    public interface IEvent : IMemberWithAccessors, IHasType
    {
        /// <summary>
        /// Gets the type of the event, i.e. the type of the delegate.
        /// </summary>
        new INamedType Type { get; }

        IMethod Signature { get; }

        /// <summary>
        /// Gets the method implementing the <c>add</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        IMethod AddMethod { get; }

        /// <summary>
        /// Gets the method implementing the <c>remove</c> semantic. In case of field-like events, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        IMethod RemoveMethod { get; }

        /// <summary>
        /// Gets an object that represents the <c>raise</c> semantic and allows to add aspects and advices
        /// as with a normal method.
        /// </summary>
        IMethod? RaiseMethod { get; }

        /// <summary>
        /// Gets an object that allows to add or remove a handler to or from the current event. 
        /// </summary>
        IInvokerFactory<IEventInvoker> Invokers { get; }

        /// <summary>
        /// Gets the base event that is overridden by the current event.
        /// </summary>
        IEvent? OverriddenEvent { get; }

        /// <summary>
        /// Gets a list of interface events this event explicitly implements.
        /// </summary>
        IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets an <see cref="EventInfo"/> that represents the current event at run time.
        /// </summary>
        /// <returns>An <see cref="EventInfo"/> that can be used only in run-time code.</returns>
        EventInfo ToEventInfo();
    }
}