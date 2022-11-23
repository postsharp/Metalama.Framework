// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Metrics
{
    /// <summary>
    /// A static class that registers all system metric providers.
    /// </summary>
    internal static class MetricProviders
    {
        public static ServiceProvider<IProjectService> WithMetricProviders( this ServiceProvider<IProjectService> serviceProvider )
            => serviceProvider.WithServices( new StatementNumberMetricProvider(), new SyntaxNodeNumberMetricProvider() );
    }
}