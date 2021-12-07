// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;

namespace Caravela.Framework.Impl.Metrics
{
    /// <summary>
    /// A static class that registers all system metric providers.
    /// </summary>
    internal static class MetricProviders
    {
        public static ServiceProvider WithMetricProviders( this ServiceProvider serviceProvider )
            => serviceProvider.WithServices( new StatementNumberMetricProvider(), new SyntaxNodeNumberMetricProvider() );
    }
}