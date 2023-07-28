// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        private sealed class FindYieldVisitor : SafeSyntaxVisitor<bool>
        {
            public static readonly FindYieldVisitor Instance = new();

            private FindYieldVisitor() { }

            public override bool VisitYieldStatement( YieldStatementSyntax node ) => true;

            public override bool DefaultVisit( SyntaxNode node )
                => node switch
                {
                    ExpressionSyntax => false,
                    LocalFunctionStatementSyntax => false,
                    _ => node.ChildNodes().Any( this.Visit )
                };
        }
    }
}