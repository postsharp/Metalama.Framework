// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static partial class IteratorHelper
    {
        /// <summary>
        /// Finds a 'yield' statement in the body.
        /// </summary>
        private class FindYieldVisitor : SafeSyntaxVisitor<bool>
        {
            public static readonly FindYieldVisitor Instance = new();

            private FindYieldVisitor() { }

            public override bool VisitYieldStatement( YieldStatementSyntax node ) => true;

            public override bool DefaultVisit( SyntaxNode node )
                => node switch
                {
                    ExpressionSyntax => false,
                    _ => node.ChildNodes().Any( this.Visit )
                };
        }
    }
}