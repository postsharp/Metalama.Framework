// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Metalama.Framework.Engine.Formatting;

public static partial class OutputCodeFormatter
{
    private sealed class CustomerSimplifier : SafeSyntaxRewriter
    {
        public override SyntaxNode? VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
        {
            if ( node.HasAnnotation( Simplifier.Annotation ) && node.ArgumentList?.Arguments is [{ Expression: AnonymousFunctionExpressionSyntax anonymousFunctionExpression }]
                                                             && node.Parent?.Kind() is SyntaxKind.Argument or SyntaxKind.SimpleAssignmentExpression )
            {
                return anonymousFunctionExpression;
            }
            else
            {
                return node;
            }
        }
    }
}