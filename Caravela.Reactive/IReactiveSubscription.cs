// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Reactive
{
    /// <summary>
    /// Represents a subscription to an <see cref="IReactiveObservable{T}"/>. The main feature
    /// is that a subscription can be disposed. This is a weakly typed variant of <see cref="IReactiveSubscription{T}"/>.
    /// </summary>
    public interface IReactiveSubscription : IDisposable
    {
        /// <summary>
        /// Gets the source object (but not necessarily the implementation of <see cref="IReactiveObservable{T}"/>.
        /// </summary>
        object Sender { get; }

        /// <summary>
        /// Gets the <see cref="IReactiveObserver"/>.
        /// </summary>
        IReactiveObserver Observer { get; }
    }

    /// <summary>
    /// Represents a subscription to an <see cref="IReactiveObservable{T}"/>. The main feature
    /// is that a subscription can be disposed. This is a strongly typed variant of <see cref="IReactiveSubscription"/>.
    /// </summary>
    public interface IReactiveSubscription<out T> : IReactiveSubscription
        where T : IReactiveObserver
    {
        /// <summary>
        /// Gets the <see cref="IReactiveObserver"/>.
        /// </summary>
        new T Observer { get; }
    }
}