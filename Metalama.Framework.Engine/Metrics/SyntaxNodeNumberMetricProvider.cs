// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Metrics;

namespace Metalama.Framework.Engine.Metrics
{
    /// <summary>
    /// A prototype implementation of <see cref="StatementNumberMetric"/>.
    /// </summary>
    internal partial class SyntaxNodeNumberMetricProvider : SyntaxMetricProvider<SyntaxNodeNumberMetric>
    {
        public SyntaxNodeNumberMetricProvider() : base( Visitor.Instance ) { }

        protected override void Aggregate( ref SyntaxNodeNumberMetric aggregate, in SyntaxNodeNumberMetric newValue ) => aggregate.Add( newValue );
    }
}