// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class PropertyGetAssignmentInliner : PropertyGetInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax needs to be in form: <variable> = <annotated_property_expression>;
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

            // The assignment should be part of expression statement.
            if ( assignmentExpression.Parent == null || assignmentExpression.Parent is not ExpressionStatementSyntax )
            {
                return false;
            }

            // Assignment should be simple and property access should be on the right.
            if ( assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression
                 || assignmentExpression.Right != aspectReference.Expression )
            {
                return false;
            }

            // Assignment should have a local on the left (TODO: ref returns).
            if ( assignmentExpression.Left is not IdentifierNameSyntax || semanticModel.GetSymbolInfo( assignmentExpression.Left ).Symbol is not ILocalSymbol )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ResolvedAspectReference aspectReference, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var assignmentExpression = (AssignmentExpressionSyntax) aspectReference.Expression.Parent.AssertNotNull();
            var localVariable = (IdentifierNameSyntax) assignmentExpression.Left.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            var targetSymbol =
                aspectReference.ResolvedSemantic.Symbol as IPropertySymbol
                ?? (IPropertySymbol) ((aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol).AssertNotNull();

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol.GetMethod.AssertNotNull(), localVariable.Identifier.ValueText );

            // Get the final inlined body of the target property getter. 
            var inlinedTargetBody =
                contextWithLocal.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull().ToSemantic( aspectReference.ResolvedSemantic.Kind ) );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}