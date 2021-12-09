// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Metrics
{
    /// <summary>
    /// Implements the computation or reading of a metric.
    /// </summary>
    /// <typeparam name="T">Type of the metric handled by the current provider.</typeparam>
    public interface IMetricProvider<out T> : IService
        where T : IMetric
    {
        /// <summary>
        /// Computes and returns the metric for a given object.
        /// </summary>
        /// <param name="measurable">An object on which the metric is defined.</param>
        /// <returns>The metric value for <paramref name="measurable"/>.</returns>
        T GetMetric( IMeasurable measurable );
    }
}