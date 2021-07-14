﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    /// <summary>
    /// Handles inlining of return statement which invokes an annotated expression.
    /// </summary>
    internal class MethodDiscardInliner : MethodInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[] { SyntaxKind.ReturnStatement };

        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: <local> = <annotated_method_expression( <arguments> );
            if ( aspectReference.ResolvedSymbol is not IMethodSymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Invocation should be on the right.
            if ( assignmentExpression.Right != invocationExpression )
            {
                return false;
            }

            // Assignment should have a discard identifier on the left (TODO: ref returns).
            if ( assignmentExpression.Left is not IdentifierNameSyntax identifierName || identifierName.Identifier.ValueText != "_" )
            {
                return false;
            }

            // The assignment should be part of expression statement.
            if ( assignmentExpression.Parent == null || assignmentExpression.Parent is not ExpressionStatementSyntax )
            {
                return false;
            }

            // The invocation needs to be inlineable in itself.
            if ( IsInlineableInvocation( semanticModel, (IMethodSymbol) aspectReference.ContainingSymbol, invocationExpression ) )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var invocationExpression = (InvocationExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var assignmentExpression = (AssignmentExpressionSyntax) invocationExpression.Parent.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            var targetSymbol = (aspectReference.ResolvedSymbol as IMethodSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithDiscard = context.WithDiscard( targetSymbol );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithDiscard.GetLinkedBody( targetSymbol );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            // We're replacing the whole return statement.
            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}