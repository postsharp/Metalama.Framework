// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Reactive
{
    /// <summary>
    /// Interface of an object that is able to collect dependencies and side values.
    /// </summary>
    public interface IReactiveCollector
    {
        /// <summary>
        /// Adds a dependency.
        /// </summary>
        /// <param name="source">The object on which the current execution context has a dependency.</param>
        /// <param name="version">The version of <paramref name="source"/> on which a dependency was taken.</param>
        void AddDependency( IReactiveObservable<IReactiveObserver> source, int version );

        /// <summary>
        /// Adds a single side value.
        /// </summary>
        /// <param name="value"></param>
        void AddSideValue( IReactiveSideValue value );

        /// <summary>
        /// Adds many side values.
        /// </summary>
        /// <param name="values"></param>
        void AddSideValues( ReactiveSideValues values );
    }
}