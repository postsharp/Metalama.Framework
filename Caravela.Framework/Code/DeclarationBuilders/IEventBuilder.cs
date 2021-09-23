// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Allows to complete the construction of an event that has been created by an advice.
    /// </summary>
    public interface IEventBuilder : IMemberBuilder, IEvent
    {
        /// <summary>
        /// Gets or sets the event type (i.e. the type of the delegates handled by this event).
        /// </summary>
        new INamedType Type { get; set; }

        /// <summary>
        /// Gets the <see cref="IMethodBuilder"/> for the event adder.
        /// </summary>
        new IMethodBuilder AddMethod { get; }

        /// <summary>
        /// Gets the <see cref="IMethodBuilder"/> for the event remover.
        /// </summary>
        new IMethodBuilder RemoveMethod { get; }

        /// <summary>
        /// Gets the <see cref="IMethodBuilder"/> for the event raiser.
        /// </summary>
        new IMethodBuilder? RaiseMethod { get; }
    }
}