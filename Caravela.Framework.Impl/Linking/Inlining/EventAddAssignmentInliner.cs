// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class EventAddAssignmentInliner : EventInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            // The syntax needs to be in form: <annotated_property_expression> += value;
            if ( aspectReference.ResolvedSemantic.Symbol is not IEventSymbol 
                && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IEventSymbol )
            {
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Property access should be on the left.
            if ( assignmentExpression.Left != aspectReference.Expression )
            {
                return false;
            }

            // Assignment should have a "value" identifier on the right (TODO: ref returns).
            if ( assignmentExpression.Right is not IdentifierNameSyntax rightIdentifier ||
                 !string.Equals( rightIdentifier.Identifier.ValueText, "value", StringComparison.Ordinal ) )
            {
                return false;
            }

            // Assignment should be simple.
            if ( assignmentExpression.Kind() != SyntaxKind.AddAssignmentExpression )
            {
                return false;
            }

            // The assignment should be part of expression statement.
            if ( assignmentExpression.Parent == null || assignmentExpression.Parent is not ExpressionStatementSyntax )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var assignmentExpression = (AssignmentExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            var targetSymbol =
                aspectReference.ResolvedSemantic.Symbol as IEventSymbol
                ?? (IEventSymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Get the final inlined body of the target property setter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.AddMethod.AssertNotNull().ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}