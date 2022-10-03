// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Metrics
{
    internal partial class SyntaxNodeNumberMetricProvider
    {
        /// <summary>
        /// A visitor that counts the syntax nodes.
        /// </summary>
        private class Visitor : SafeSyntaxVisitor<SyntaxNodeNumberMetric>
        {
            public static readonly Visitor Instance = new();

            public override SyntaxNodeNumberMetric DefaultVisit( SyntaxNode node )
            {
                var metric = new SyntaxNodeNumberMetric { Value = 1 };

                foreach ( var child in node.ChildNodes() )
                {
                    metric.Add( this.Visit( child ) );
                }

                return metric;
            }
        }
    }
}