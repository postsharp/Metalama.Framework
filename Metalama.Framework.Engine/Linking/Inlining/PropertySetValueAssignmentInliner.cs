// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
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

            if ( aspectReference.SourceExpression.Parent == null
                 || aspectReference.SourceExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Should be simple assignment and property access should be on the left.
            if ( assignmentExpression.Kind() != SyntaxKind.SimpleAssignmentExpression
                 || assignmentExpression.Left != aspectReference.SourceExpression )
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

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            var assignmentExpression = (AssignmentExpressionSyntax) aspectReference.SourceExpression.Parent.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            return new InliningAnalysisInfo( expressionStatement, null );
        }

        public override StatementSyntax Inline(
            SyntaxGenerationContext syntaxGenerationContext,
            InliningSpecification specification,
            SyntaxNode currentNode,
            StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody;
        }
    }
}