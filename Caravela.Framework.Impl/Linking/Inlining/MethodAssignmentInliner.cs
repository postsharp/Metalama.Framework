// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class MethodAssignmentInliner : MethodInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: <local> = <annotated_method_expression( <arguments> );
            if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol )
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

            // Assignment should be simple and Invocation should be on the right.
            if ( assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression
                 || assignmentExpression.Right != invocationExpression )
            {
                return false;
            }

            // Assignment should have a local on the left.
            if ( assignmentExpression.Left is not IdentifierNameSyntax || semanticModel.GetSymbolInfo( assignmentExpression.Left ).Symbol is not ILocalSymbol )
            {
                return false;
            }

            // Assignment should be simple.
            if ( assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression )
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
            var localVariable = (IdentifierNameSyntax) assignmentExpression.Left.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            var targetSymbol = (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol, localVariable.Identifier.ValueText );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol.ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            // We're replacing the whole return statement.
            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}