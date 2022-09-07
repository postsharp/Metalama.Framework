// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Metrics;

namespace Metalama.Framework.Engine.Metrics
{
    /// <summary>
    /// A prototype implementation of <see cref="StatementNumberMetric"/>.
    /// </summary>
    internal partial class StatementNumberMetricProvider : SyntaxMetricProvider<StatementNumberMetric>
    {
        public StatementNumberMetricProvider() : base( Visitor.Instance ) { }

        protected override void Aggregate( ref StatementNumberMetric aggregate, in StatementNumberMetric newValue ) => aggregate.Add( newValue );
    }
}