// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class PropertySetValueAssignmentInliner : PropertyInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax needs to be in form: <annotated_property_expression> = value;
            if ( aspectReference.ResolvedSemantic.Symbol is not IPropertySymbol
                 && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IPropertySymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            if ( aspectReference.Expression.Parent == null || aspectReference.Expression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Should be simple assignment and property access should be on the left.
            if ( assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression
                 || assignmentExpression.Left != aspectReference.Expression )
            {
                return false;
            }

            // Assignment should have a "value" identifier on the right (TODO: ref returns).
            if ( assignmentExpression.Right is not IdentifierNameSyntax rightIdentifier ||
                 !string.Equals( rightIdentifier.Identifier.ValueText, "value", StringComparison.Ordinal ) )
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
                aspectReference.ResolvedSemantic.Symbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Get the final inlined body of the target property setter. 
            var inlinedTargetBody = context.GetLinkedBody( targetSymbol.SetMethod.AssertNotNull().ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}