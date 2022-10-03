// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Metrics
{
    internal partial class StatementNumberMetricProvider
    {
        /// <summary>
        /// A visitor that counts the syntax nodes.
        /// </summary>
        private class Visitor : SafeSyntaxVisitor<StatementNumberMetric>
        {
            public static readonly Visitor Instance = new();

            public override StatementNumberMetric DefaultVisit( SyntaxNode node )
            {
                var metric = default(StatementNumberMetric);

                foreach ( var child in node.ChildNodes() )
                {
                    metric.Add( this.Visit( child ) );
                }

                return metric;
            }

            public override StatementNumberMetric VisitIfStatement( IfStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                if ( node.Else != null )
                {
                    metric.Add( this.Visit( node.Else ) );
                }

                return metric;
            }

            public override StatementNumberMetric VisitSwitchStatement( SwitchStatementSyntax node )
            {
                var metric = default(StatementNumberMetric);

                metric.Value++;

                foreach ( var section in node.Sections )
                {
                    foreach ( var statement in section.Statements )
                    {
                        metric.Add( this.Visit( statement ) );
                    }
                }

                return metric;
            }

            public override StatementNumberMetric VisitBlock( BlockSyntax node )
            {
                var metric = default(StatementNumberMetric);

                foreach ( var statement in node.Statements )
                {
                    metric.Add( this.Visit( statement ) );
                }

                return metric;
            }

            public override StatementNumberMetric VisitWhileStatement( WhileStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitDoStatement( DoStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitForStatement( ForStatementSyntax node )
            {
                // We intentionally ignore the assignment and increment statements because we don't want to count in trivial increments and initializations.

                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitExpressionStatement( ExpressionStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitVariableDeclarator( VariableDeclaratorSyntax node )
            {
                if ( node.Initializer != null )
                {
                    return new StatementNumberMetric { Value = 1 };
                }
                else
                {
                    return default;
                }
            }

            public override StatementNumberMetric VisitBreakStatement( BreakStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitContinueStatement( ContinueStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitGotoStatement( GotoStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitThrowStatement( ThrowStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitReturnStatement( ReturnStatementSyntax node ) => new() { Value = 1 };

            public override StatementNumberMetric VisitLabeledStatement( LabeledStatementSyntax node ) => this.Visit( node.Statement );

            public override StatementNumberMetric VisitLockStatement( LockStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitUnsafeStatement( UnsafeStatementSyntax node ) => this.Visit( node.Block );

            public override StatementNumberMetric VisitTryStatement( TryStatementSyntax node )
            {
                var metric = this.Visit( node.Block );

                foreach ( var block in node.Catches )
                {
                    metric.Add( this.Visit( block.Block ) );
                }

                if ( node.Finally != null )
                {
                    metric.Add( this.Visit( node.Finally ) );
                }

                return metric;
            }

            public override StatementNumberMetric VisitUsingStatement( UsingStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitForEachStatement( ForEachStatementSyntax node )
            {
                var metric = this.Visit( node.Statement );

                metric.Value++;

                return metric;
            }

            public override StatementNumberMetric VisitYieldStatement( YieldStatementSyntax node ) => new() { Value = 1 };
        }
    }
}