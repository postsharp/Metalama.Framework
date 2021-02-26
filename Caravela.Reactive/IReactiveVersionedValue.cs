// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Reactive
{
    /// <summary>
    /// Exposes a value and a version number.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IReactiveVersionedValue<out TValue> : IHasReactiveSideValues
    {
        /// <summary>
        /// Gets the version of the <see cref="Value"/>.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the value itself.
        /// </summary>
        TValue Value { get; }
    }
}