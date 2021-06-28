// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal class PropertyGetAssignmentInliner : PropertyInliner
    {
        public override IReadOnlyList<SyntaxKind> AncestorSyntaxKinds => new[]
        {
            SyntaxKind.ReturnStatement
        };
        
        public override bool CanInline( ISymbol contextDeclaration, SemanticModel semanticModel, ExpressionSyntax annotatedExpression )
        {
            // The syntax needs to be in form: <variable> = <annotated_property_expression>;

            if ( !annotatedExpression.TryGetAspectReference( out _ ) )
            {
                throw new AssertionFailedException();
            }

            if ( contextDeclaration is not IPropertySymbol )
            {
                throw new AssertionFailedException();
            }

            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not AssignmentExpressionSyntax assignmentExpression )
            {
                return false;
            }

            if ( annotatedExpression.Parent == null || annotatedExpression.Parent is not ExpressionStatementSyntax )
            {
                return false;
            }

            // Property access should be on the right.
            if ( assignmentExpression.Right != annotatedExpression )
            { 
                return false;
            }

            // Assignment should have a local on the left (TODO: ref returns).
            if ( assignmentExpression.Left is not IdentifierNameSyntax || semanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol is not ILocalSymbol)
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

        public override void Inline( InliningContext context, ExpressionSyntax annotatedExpression, out SyntaxNode replacedNode, out SyntaxNode newNode )
        {
            var semanticModel = context.Compilation.GetSemanticModel( annotatedExpression.SyntaxTree );
            var referencedSymbol = (IPropertySymbol) semanticModel.GetSymbolInfo( annotatedExpression ).Symbol.AssertNotNull();
            var assignmentExpression = (AssignmentExpressionSyntax) annotatedExpression.Parent.AssertNotNull();
            var localVariable = (IdentifierNameSyntax) assignmentExpression.Left.AssertNotNull();
            var expressionStatement = (ExpressionStatementSyntax) assignmentExpression.Parent.AssertNotNull();

            if ( !annotatedExpression.TryGetAspectReference( out var aspectReference ) )
            {
                throw new AssertionFailedException();
            }

            // We need to change the return variable and label of the inlined code and then generate hand-over code.
            var targetSymbol = (IPropertySymbol)context.ReferenceResolver.Resolve( referencedSymbol, aspectReference );

            // Change the target local variable.
            var contextWithLocal = context.WithReturnLocal( targetSymbol.GetMethod.AssertNotNull(), localVariable.Identifier.ValueText );

            // Get the final inlined body of the target property getter. 
            var inlinedTargetBody = contextWithLocal.GetLinkedBody( targetSymbol.GetMethod.AssertNotNull() );

            // Mark the block as flattenable.
            inlinedTargetBody = inlinedTargetBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

            newNode = inlinedTargetBody;
            replacedNode = expressionStatement;
        }
    }
}
