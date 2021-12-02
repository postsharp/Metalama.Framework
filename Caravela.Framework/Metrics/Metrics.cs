// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Metrics
{
    /// <summary>
    /// Exposes a <see cref="Get{TExtension}"/> method, which computes and returns a metric for an eligible object.
    /// </summary>
    /// <typeparam name="TMeasurable">The type extended with metrics.</typeparam>
    public readonly struct Metrics<TMeasurable>
        where TMeasurable : IMeasurable
    {
        private readonly TMeasurable _extensible;

        internal Metrics( TMeasurable extensible )
        {
            this._extensible = extensible;
        }

        /// <summary>
        /// Gets a given metric for the current object.
        /// </summary>
        /// <typeparam name="TExtension">The type of metric to be returned.</typeparam>
        /// <returns></returns>
        public TExtension Get<TExtension>()
            where TExtension : IMetric<TMeasurable>
            => ((IMeasurableInternal) this._extensible).GetMetric<TExtension>();
    }
}