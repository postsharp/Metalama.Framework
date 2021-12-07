// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Metrics;

namespace Metalama.Framework.Impl.Metrics
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