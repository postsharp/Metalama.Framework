// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    internal class MethodReturnStatementInliner : MethodInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax has to be in form: return <annotated_method_expression( <arguments> );
            if ( aspectReference.ResolvedSemantic.Symbol is not IMethodSymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not ReturnStatementSyntax )
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
            var returnStatement = (ReturnStatementSyntax) invocationExpression.Parent.AssertNotNull();

            var targetSymbol = (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol).AssertNotNull();

            // Get the final body (after inlining) of the target.
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            replacedNode = returnStatement;
            newNode = inlinedTargetBody;
        }
    }
}