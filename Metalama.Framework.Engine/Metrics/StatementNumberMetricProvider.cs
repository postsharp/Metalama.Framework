// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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