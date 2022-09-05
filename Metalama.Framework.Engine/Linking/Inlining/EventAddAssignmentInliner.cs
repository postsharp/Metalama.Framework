﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal class EventAddAssignmentInliner : EventInliner
    {
        public override bool CanInline( ResolvedAspectReference aspectReference, SemanticModel semanticModel )
        {
            if ( !base.CanInline( aspectReference, semanticModel ) )
            {
                return false;
            }

            // The syntax needs to be in form: <annotated_property_expression> += value;
            if ( aspectReference.ResolvedSemantic.Symbol is not IEventSymbol
                 && (aspectReference.ResolvedSemantic.Symbol as IMethodSymbol)?.AssociatedSymbol is not IEventSymbol )
            {
                // Coverage: ignore (hit only when the check in base class is incorrect).
                return false;
            }

            if ( aspectReference.SourceExpression.Parent == null || aspectReference.SourceExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Property access should be on the left.
            if ( assignmentExpression.Left != aspectReference.SourceExpression )
            {
                return false;
            }

            // Assignment should have a "value" identifier on the right.
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
                // Only incorrect code can get here.
                throw new AssertionFailedException( Justifications.CoverageMissing );

                // return false;
            }

            return true;
        }

        public override InliningAnalysisInfo GetInliningAnalysisInfo( InliningAnalysisContext context, ResolvedAspectReference aspectReference )
        {
            var assignmentExpression = (AssignmentExpressionSyntax) aspectReference.SourceExpression.Parent.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            return new InliningAnalysisInfo( expressionStatement, null );
        }

        public override StatementSyntax Inline( SyntaxGenerationContext syntaxGenerationContext, InliningSpecification specification, SyntaxNode currentNode, StatementSyntax linkedTargetBody )
        {
            return linkedTargetBody;
        }
    }
}