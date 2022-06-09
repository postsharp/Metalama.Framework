// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static partial class IteratorHelper
    {
        /// <summary>
        /// Finds a 'yield' statement in the body.
        /// </summary>
        private class FindYieldVisitor : CSharpSyntaxVisitor<bool>
        {
            public static readonly FindYieldVisitor Instance = new();

            private FindYieldVisitor() { }

            public override bool VisitYieldStatement( YieldStatementSyntax node ) => true;

            public override bool VisitBlock( BlockSyntax node ) => node.Statements.Any( s => this.Visit( s ) );

            public override bool VisitForEachStatement( ForEachStatementSyntax node ) => this.Visit( node.Statement );

            public override bool VisitForStatement( ForStatementSyntax node ) => this.Visit( node.Statement );

            public override bool VisitWhileStatement( WhileStatementSyntax node ) => this.Visit( node.Statement );

            public override bool VisitDoStatement( DoStatementSyntax node ) => this.Visit( node.Statement );

            public override bool VisitSwitchStatement( SwitchStatementSyntax node ) => node.Sections.Any( s => this.Visit( s ) );

            public override bool VisitSwitchSection( SwitchSectionSyntax node ) => node.Statements.Any( s => this.Visit( s ) );

            // TODO: Probably something else.

            public override bool VisitIfStatement( IfStatementSyntax node )
                => this.Visit( node.Statement ) || this.Visit( node.Else );

            public override bool VisitElseClause( ElseClauseSyntax node )
                => this.Visit( node.Statement );

            public override bool DefaultVisit( SyntaxNode node ) => false;
        }
    }
}