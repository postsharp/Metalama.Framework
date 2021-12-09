// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Metrics
{
    internal partial class SyntaxNodeNumberMetricProvider
    {
        /// <summary>
        /// A visitor that counts the syntax nodes.
        /// </summary>
        private class Visitor : CSharpSyntaxVisitor<SyntaxNodeNumberMetric>
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