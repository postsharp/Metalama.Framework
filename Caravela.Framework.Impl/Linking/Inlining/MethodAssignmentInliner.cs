﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class MethodAssignmentInliner : MethodInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[]
        {
            SyntaxKind.ReturnStatement
        };
        
        public override bool CanInline( IMethodSymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression )
        {
            // The syntax has to be in form: <local> = <annotated_method_expression( <arguments> );
            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not InvocationExpressionSyntax invocationExpression )
            {
                return false;
            }

            if ( invocationExpression.Parent == null || invocationExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            // Invocation should be on the right.
            if ( assignmentExpression.Right != invocationExpression)
            {
                return false;
            }

            // Assignment should have a local on the left.
            if ( assignmentExpression.Left is not IdentifierNameSyntax || semanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol is not ILocalSymbol)
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
            if ( IsInlineableInvocation( semanticModel, contextDeclaration, invocationExpression ) )
            {
                return false;
            }

            return true;
        }

        public override void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var semanticModel = context.Compilation.GetSemanticModel( annotatedExpression.SyntaxTree );
            var referencedSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo( annotatedExpression ).Symbol.AssertNotNull();
            var invocationExpression = (InvocationExpressionSyntax) annotatedExpression.Parent.AssertNotNull();
            var assignmentExpression = (AssignmentExpressionSyntax) invocationExpression.Parent.AssertNotNull();
            var localVariable = (IdentifierNameSyntax) assignmentExpression.Left.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            if ( !annotatedExpression.TryGetAspectReference( out var aspectReference ) )
            {
                throw new AssertionFailedException();
            }

            // We need to change the return variable and label of the inlined code and then generate hand-over code.
            var targetSymbol = (IMethodSymbol) context.ReferenceResolver.Resolve( referencedSymbol, aspectReference );

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol, localVariable.Identifier.ValueText );

            // Get the final inlined body of the target method. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            // We're replacing the whole return statement.
            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}
