// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Reactive
{
    /// <summary>
    /// An observable is something to which observers (<see cref="IReactiveObserver"/>) can subscribe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveObservable<in T>
        where T : IReactiveObserver
    {
        /// <summary>
        /// Gets the current version of the observable object.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the original object, which may be different to the helper object that implements the interface.
        /// </summary>
        IReactiveSource Source { get; }

        /// <summary>
        /// Adds an observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>An object that allows unsubscription either using <see cref="IDisposable.Dispose"/>
        ///    or <see cref="RemoveObserver"/>, or <c>null</c> if the current source is immutable.
        /// </returns>
        IReactiveSubscription? AddObserver( T observer );

        bool RemoveObserver( IReactiveSubscription subscription );
    }
}