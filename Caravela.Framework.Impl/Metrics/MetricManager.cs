// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Metrics;
using Caravela.Framework.Project;
using System;
using System.Collections.Concurrent;

namespace Caravela.Framework.Impl.Metrics
{
    /// <summary>
    /// Manages the metric providers and route metric requests to them.
    /// </summary>
    public sealed class MetricManager : IService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, object?> _metricProviders = new();

        internal MetricManager( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets a given metric for a given object.
        /// </summary>
        /// <param name="measurable">The object for which the metric is requested.</param>
        /// <typeparam name="T">The type of requested object.</typeparam>
        public T GetMetric<T>( IMeasurable measurable )
            where T : IMetric
        {
            var provider = this._metricProviders.GetOrAdd( typeof(T), _ => this.GetExtensionProvider<T>() );

            if ( provider == null )
            {
                throw new InvalidOperationException( $"No extension provider registered for {typeof(T).Name}" );
            }

            return ((IMetricProvider<T>) provider).GetMetric( measurable );
        }

        private object? GetExtensionProvider<T>()
            where T : IMetric
            => this._serviceProvider.GetOptionalService<IMetricProvider<T>>();
    }
}