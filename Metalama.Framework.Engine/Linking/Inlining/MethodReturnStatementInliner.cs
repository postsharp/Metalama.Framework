﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    /// <summary>
    /// Handles inlining of return statement which invokes an annotated expression.
    /// </summary>
    internal class MethodReturnStatementInliner : MethodInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax has to be in form: return <annotated_method_expression( <arguments> );
            if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol methodSymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( !SymbolEqualityComparer.Default.Equals(
                    methodSymbol.ReturnType,
                    ((IMethodSymbol) aspectReference.ContainingSymbol).ReturnType ) )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not ReturnStatementSyntax )
            {
                return false;
            }

            // The invocation needs to be inlineable in itself.
            if ( !IsInlineableInvocation( semanticModel, (IMethodSymbol) aspectReference.ContainingSymbol, invocationExpression ) )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var invocationExpression = (InvocationExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var returnStatement = (ReturnStatementSyntax) invocationExpression.Parent.AssertNotNull();

            var targetSymbol = (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol).AssertNotNull();

            // Get the final body (after inlining) of the target.
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            var a = returnStatement.GetLeadingTrivia();
            var b = inlinedTargetBody.GetLeadingTrivia();
            var c = b.InsertRange( 0, a );

            // Add the original trivia and mark the block as flattenable.
            // This will skip any trivia within the return statement as we don't have a place to add them.
            inlinedTargetBody = 
                inlinedTargetBody
                .WithOpenBraceToken( 
                    inlinedTargetBody.OpenBraceToken
                    .WithTrailingTrivia( inlinedTargetBody.OpenBraceToken.TrailingTrivia.InsertRange( 0, returnStatement.GetLeadingTrivia() ) ) )
                .WithCloseBraceToken(
                    inlinedTargetBody.CloseBraceToken
                    .WithLeadingTrivia( inlinedTargetBody.CloseBraceToken.LeadingTrivia.AddRange( returnStatement.GetTrailingTrivia() ) ) )
                .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            replacedNode = returnStatement;
            newNode = inlinedTargetBody;
        }
    }
}